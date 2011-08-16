﻿namespace PiotrTrzpil.VisualMutator_VSPackage.Model.Mutations
{
    #region Usings

    using System.Collections.ObjectModel;

    using VisualMutator.Extensibility;

    #endregion

    public class OperatorPackage : TreeElement
    {
        private readonly IOperatorsPack _operatorsPack;

        private  ObservableCollection<MutationOperator> _operators;

        public OperatorPackage(IOperatorsPack operatorsPack)
        {
            _operatorsPack = operatorsPack;
            Name = "pack";
            Operators = new ObservableCollection<MutationOperator>();
        }

        public ObservableCollection<MutationOperator> Operators
        {
            set
            {
                if (_operators != value)
                {
                    _operators = value;
                    RaisePropertyChanged(() => Operators);
                }
            }
            get
            {
                return _operators;
            }
        }
    }
}