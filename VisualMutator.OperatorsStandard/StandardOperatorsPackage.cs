﻿namespace VisualMutator.MvcMutations
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using System.Text;

    using VisualMutator.Extensibility;
    using VisualMutator.OperatorsStandard;

    [PackageExport]
    public class StandardOperatorsPackage : IOperatorsPackage
    {
        public StandardOperatorsPackage()
        {
            Operators = new IMutationOperator[]
            {
  
                new AOR_ArithmeticOperatorReplacement(), 
                new ABS_AbsoluteValueInsertion(),
                new SOR_ShiftOperatorReplacement(), 
                new UOI_UnaryOperatorInsertion(), 
                new LOR_LogicalOperatorReplacement(), 
                new ROR_RelationalOperatorReplacement(), 

            };
        }
        public IEnumerable<IMutationOperator> Operators
        {
            get; 
            set;
        }
        public string Name
        {
            get
            {
                return "Standard";
            }
        }
        public string Description
        {
            get
            {
                return "Standard imperative operators.";
            }
        }
    }
}