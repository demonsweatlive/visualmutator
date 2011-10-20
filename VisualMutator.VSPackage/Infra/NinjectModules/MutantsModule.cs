﻿namespace PiotrTrzpil.VisualMutator_VSPackage.Infrastructure.NinjectModules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Ninject.Modules;

    using PiotrTrzpil.VisualMutator_VSPackage.Views;

    using VisualMutator.Controllers;
    using VisualMutator.Model.Mutations;
    using VisualMutator.Model.Mutations.Operators;
    using VisualMutator.Model.Mutations.Types;
    using VisualMutator.ViewModels;
    using VisualMutator.Views;

    public class MutantsModule : NinjectModule 
    {
        public override void Load()
        {

            Kernel.Bind<IILMutationsView>().To<ILMutationsView>().InSingletonScope();
            Kernel.Bind<MutantsCreationController>().ToSelf().AndFromFactory();
            Kernel.Bind<MutantsCreationViewModel>().ToSelf();
            Kernel.Bind<IMutantsCreationView>().To<MutantsCreationWindow>();

            Kernel.Bind<MutationResultsController>().ToSelf().InSingletonScope();
            Kernel.Bind<MutationResultsViewModel>().ToSelf();
            Kernel.Bind<IMutationResultsView>().To<MutationResultsView>();

            Kernel.Bind<IMutantsContainer>().To<MutantsContainer>().InSingletonScope();
            Kernel.Bind<IMutantsFileManager>().To<MutantsFileManager>().InSingletonScope();

           
           // Kernel.Bind<ILMutationsViewModel>().ToSelf().InSingletonScope();

        
            Kernel.Bind<MutantsManagementViewModel>().ToSelf().AndFromFactory();
            Kernel.Bind<IMutantsManagementView>().To<MutantsManagementView>();


            Kernel.Bind<MutantDetailsController>().ToSelf();
            Kernel.Bind<MutantDetailsViewModel>().ToSelf();
            Kernel.Bind<IMutantDetailsView>().To<MutantDetailsView>();






            Kernel.Bind<IAssemblyReaderWriter>().To<AssemblyReaderWriter>().InSingletonScope();
            Kernel.Bind<ITypesManager>().To<SolutionTypesManager>().InSingletonScope();
            Kernel.Bind<IOperatorsManager>().To<OperatorsManager>().InSingletonScope();
            Kernel.Bind<IOperatorLoader>().To<MEFOperatorLoader>().InSingletonScope();

            Kernel.InjectFuncFactory(() => DateTime.Now);

        }
    }
}