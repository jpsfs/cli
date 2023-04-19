﻿using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions.Processors;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;
using Cmf.CLI.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Processors;

internal class OrchestrationEndMethodProcessor : BaseProcessor, IOrchestrationEndMethodProcessor
{
    public override bool IsStartMethod => false;

    public override void Process(SyntaxNode statement)
    {
        base.Process(statement);

        var statementArguments = statement.DescendantNodes().OfType<ArgumentListSyntax>().FirstOrDefault();

        if (statementArguments is null)
        {
            return;
        }

        // TODO: Validate EntityTypeId

        // TODO: Validate EntityId

        ValidateOutputParameter(statementArguments.Arguments.Select(x => x.ChildNodes()).Aggregate((l1, l2) => l1.Concat(l2)));
    }

    public override void ValidateParameterCount(ArgumentListSyntax statementArguments)
    {
        if (statementArguments.Arguments.Count != 3 + _parameterNames.Count)
        {
            Log.Warning(string.Format(ErrorMessages.MethodHasIncorrectNumberOfParameters, StartEndMethodString, _namespaceName, _className, _methodName, statementArguments.Arguments.Count, 2 + _parameterNames.Count));
        }
    }

    private void ValidateOutputParameter(IEnumerable<SyntaxNode> arguments)
    {
        if (_outputType.EndsWith("Output"))
        {
			foreach (var objectArgument in arguments)
			{
				if (objectArgument.IsKind(SyntaxKind.ObjectCreationExpression))
				{
					var ObjectArgumentList = objectArgument.ChildNodes().Where(x => x.IsKind(SyntaxKind.ArgumentList)).FirstOrDefault() as ArgumentListSyntax;
					var dictionaryKey = ObjectArgumentList is null ? string.Empty : ObjectArgumentList.Arguments[0].ToString().Trim('"');
					var dictionaryValue = ObjectArgumentList?.Arguments[1].ToString();

					if (dictionaryKey.EqualsToItselfOrNameOfItself(_outputType))
					{
						return;
					}
				}
			}

			Log.Warning(string.Format(ErrorMessages.MethodParameterMissingOrIncorrectlyNamed, StartEndMethodString, _namespaceName, _className, _methodName, _outputType));
		}        
    }
}