﻿namespace VisualMutator.Model.Mutations.Types
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Exceptions;
    using Infrastructure;
    using log4net;
    using Microsoft.Cci;
    using UsefulTools.CheckboxedTree;
    using UsefulTools.ExtensionMethods;
    using UsefulTools.Paths;

    #endregion

    public interface ITypesManager
    {

        IList<AssemblyNode> GetTypesFromAssemblies(IList<FilePathAbsolute> paths);

        LoadedTypes GetIncludedTypes(IEnumerable<AssemblyNode> assemblies);

        bool IsAssemblyLoadError { get; set; }

        IEnumerable<DirectoryPathAbsolute> ProjectPaths { get; }
    }

    public class SolutionTypesManager : ITypesManager
    {
        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IModuleSource _moduleSource;

        private readonly IHostEnviromentConnection _hostEnviroment;

        private readonly IEnumerable<DirectoryPathAbsolute> _projectPaths;

        public IEnumerable<DirectoryPathAbsolute> ProjectPaths
        {
            get
            {
                return _projectPaths;
            }
        }

        public bool IsAssemblyLoadError { get; set; }
   
        public SolutionTypesManager(
            IModuleSource moduleSource,
            IHostEnviromentConnection hostEnviroment)
        {
            _moduleSource = moduleSource;
            _hostEnviroment = hostEnviroment;

            _projectPaths = _hostEnviroment.GetProjectPaths();
        }


        public LoadedTypes GetIncludedTypes(IEnumerable<AssemblyNode> assemblies)
        {
            var types = assemblies
                .SelectManyRecursive<CheckedNode>(node => node.Children, node => node.IsIncluded ?? true, leafsOnly:true)
                .Cast<TypeNode>().Select(type=>type.TypeDefinition).ToList();
            return new LoadedTypes(types);
        }
       
        public IList<AssemblyNode> GetTypesFromAssemblies(IList<FilePathAbsolute> paths)
        {

            var loadedAssemblies = LoadAssemblies(paths);
            var root = new RootNode();
            root.Children.AddRange(loadedAssemblies);
            root.IsIncluded = true;

            return loadedAssemblies;
        }

        private IList<AssemblyNode> LoadAssemblies(IEnumerable<FilePathAbsolute> assembliesPaths)
        {
            var assemblyTreeNodes = new List<AssemblyNode>();
            foreach (FilePathAbsolute assemblyPath in assembliesPaths)
            {
                
                try
                {
                    IModule module = _moduleSource.AppendFromFile((string)assemblyPath);
                   
                    var assemblyNode = new AssemblyNode(module.Name.Value, module);
                    assemblyNode.AssemblyPath = assemblyPath;

                    GroupTypes(assemblyNode, "", ChooseTypes(module).ToList());


                    assemblyTreeNodes.Add(assemblyNode);

                }
                catch (AssemblyReadException e)
                {
                    _log.Info("ReadAssembly failed. ", e);
                    IsAssemblyLoadError = true;
                }
                catch (Exception e)
                {
                    _log.Info("ReadAssembly failed. ", e);
                    IsAssemblyLoadError = true;
                }
            }
            return assemblyTreeNodes;
        }
        //TODO: nessessary?
        private static IEnumerable<INamespaceTypeDefinition> ChooseTypes(IModule module)
        {
            return module.GetAllTypes()
                .OfType<INamespaceTypeDefinition>()
                .Where(t => t.Name.Value != "<Module>")
                .Where(t => !t.Name.Value.StartsWith("<>"));

        }

        public void GroupTypes(CheckedNode parent, string currentNamespace, ICollection<INamespaceTypeDefinition> types)
        {
            var groupsByNamespaces = types
                .Where(t => t.ContainingNamespace.Name.Value != currentNamespace)
                .OrderBy(t => t.ContainingNamespace.Name.Value)
                .GroupBy(t => ExtractNextNamespacePart(t.ContainingNamespace.Name.Value, currentNamespace))
                .ToList();

            var leafTypes = types
                .Where(t => t.ContainingNamespace.Name.Value == currentNamespace)
                .OrderBy(t => t.Name.Value)
                .ToList();

            // Maybe we can merge namespace nodes:
            if (currentNamespace != "" && groupsByNamespaces.Count == 1 && !leafTypes.Any())
            {
                var singleGroup = groupsByNamespaces.Single();
                parent.Name = ConcatNamespace(parent.Name, singleGroup.Key);
                GroupTypes(parent, ConcatNamespace(currentNamespace, singleGroup.Key), singleGroup.ToList());
            }
            else
            {
                foreach (var typesGroup in groupsByNamespaces)
                {
                    var node = new TypeNamespaceNode(parent, typesGroup.Key);
                    GroupTypes(node, ConcatNamespace(currentNamespace, typesGroup.Key), typesGroup.ToList());
                    parent.Children.Add(node);
                }

                foreach (INamespaceTypeDefinition typeDefinition in leafTypes)
                {
                    parent.Children.Add(new TypeNode(parent, typeDefinition.Name.Value, typeDefinition));
                  
                }
            }
        }

        public string ConcatNamespace(string one, string two)
        {
            return one == "" ? two : one + "." + two;
        }

        public string ExtractNextNamespacePart(string extractFrom, string namespaceName)
        {
            if (!extractFrom.StartsWith(namespaceName))
            {
                throw new ArgumentException("extractFrom");
            }

            if (namespaceName != "")
            {
                extractFrom = extractFrom.Remove(
                    0, namespaceName.Length + 1);
            }

            int index = extractFrom.IndexOf('.');
            return index != -1 ? extractFrom.Remove(extractFrom.IndexOf('.')) : extractFrom;
        }
    }
}