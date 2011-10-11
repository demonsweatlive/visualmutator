﻿namespace VisualMutator.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using CommonUtilityInfrastructure;
    using CommonUtilityInfrastructure.WpfUtils;

    using VisualMutator.Infrastructure.Factories;
    using VisualMutator.Model.Mutations;
    using VisualMutator.Model.Mutations.Types;
    using VisualMutator.Model.Tests;
    using VisualMutator.ViewModels;

    public class ExecutedOperator
    {
        public List<Mutant> Mutants { get; set; }

        public string Name
        {
            get; set;
        }
    }

    public class MutationResultsController : Controller
    {
        private readonly MutationResultsViewModel _viewModel;

        private readonly IFactory<MutantsCreationController> _mutantsCreationFactory;

        private readonly IMutantsContainer _mutantsContainer;

        private readonly ITestsContainer _testsContainer;

        private readonly Services _services;

        public MutationResultsController(
            MutationResultsViewModel viewModel,
            IFactory<MutantsCreationController> mutantsCreationFactory,
            IMutantsContainer mutantsContainer,
            ITestsContainer testsContainer,
            Services services)
        {
            _viewModel = viewModel;
            _mutantsCreationFactory = mutantsCreationFactory;
            _mutantsContainer = mutantsContainer;
            _testsContainer = testsContainer;
            _services = services;

            _viewModel.CommandCreateNewMutants = new BasicCommand(CreateMutants, () => !_viewModel.AreOperationsOngoing);
            _viewModel.CommandCreateNewMutants.ExecuteOnChanged(_viewModel, () => _viewModel.AreOperationsOngoing);

            _viewModel.CommandStop = new BasicCommand(Stop, () => _viewModel.AreOperationsOngoing);
            _viewModel.CommandStop.ExecuteOnChanged(_viewModel, () => _viewModel.AreOperationsOngoing);

            _viewModel.Operators = new BetterObservableCollection<ExecutedOperator>();
        }

       
        

        public void CreateMutants()
        {
            var mutantsCreationController = _mutantsCreationFactory.Create();
            mutantsCreationController.Run();
            MutationSessionChoices choices = mutantsCreationController.Result;

            _viewModel.OperationsStateDescription = "Creating mutants...";
            _viewModel.AreOperationsOngoing = true;

            var tasks = choices.SelectedOperators.Select(op =>
            {
                return _services.Threading.ScheduleAsync(() => 
                    _mutantsContainer.GenerateMutantsForOperator(choices, op),
                    operatorWithMutants => _viewModel.Operators.Add(operatorWithMutants));

            }).ToArray();


            Task.Factory.ContinueWhenAll(tasks, tasks2 => RunTests());
        }


        public void RunTests()
        {
            _viewModel.OperationsStateDescription = "Running tests...";
           /* var tasks = _viewModel.Operators.SelectMany(op => op.Mutants).Select(mut =>
            {
            //    return _services.Threading.ScheduleAsync(() => _testsContainer.RunTestsForMutant(mut),
                   // operatorWithMutants => _viewModel.Operators.Add(operatorWithMutants));
            }).ToArray();*/
        }

        public void Stop()
        {

        }

        public void Initialize()
        {
            
        }

        public void Deactivate()
        {
            Stop();
            Clean();
        }

        private void Clean()
        {
            _viewModel.Operators.Clear();
        }

        public MutationResultsViewModel ViewModel
        {
            get
            {
                return _viewModel;
            }
        }
    }
}