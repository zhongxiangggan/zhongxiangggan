// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions
{
    internal sealed class EditMetadataByCreatingNewTypeAttributePass : IntermediateNodePassBase, IRazorOptimizationPass
    {
        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            var @namespace = documentNode.FindPrimaryNamespace();
            var @class = documentNode.FindPrimaryClass();
        
            var classIndex = @namespace.Children.IndexOf(@class);
            if (classIndex == -1)
            {
                return;
            }

            var metadataAttributeNode = new CreateNewOnMetadataUpdateAttributeIntermediateNode();
            // Metadata attributes need to be inserted right before the class declaration.
            @namespace.Children.Insert(classIndex, metadataAttributeNode);
        }

        internal sealed class CreateNewOnMetadataUpdateAttributeIntermediateNode : ExtensionIntermediateNode
        {
            private const string CreateNewOnMetadataUpdateAttributeName = "global::System.Runtime.CompilerServices.CreateNewOnMetadataUpdateAttribute";

            public override IntermediateNodeCollection Children => IntermediateNodeCollection.ReadOnly;

            public override void Accept(IntermediateNodeVisitor visitor)
            {
                AcceptExtensionNode(this, visitor);
            }

            public override void WriteNode(CodeTarget target, CodeRenderingContext context)
            {
                // [global::System.Runtime.CompilerServices.CreateNewOnMetadataUpdateAttribute]
                context.CodeWriter.Write("[");
                context.CodeWriter.Write(CreateNewOnMetadataUpdateAttributeName);
                context.CodeWriter.WriteLine("]");
            }
        }
    }
}
