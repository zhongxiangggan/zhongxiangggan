// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions
{
    /// <summary>
    /// Applies attributes required for hot reload.
    /// </summary>
    internal sealed class ApplyUpdateAttributePass : IntermediateNodePassBase, IRazorOptimizationPass
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

            var identifierFeature = Engine.Features.OfType<IMetadataIdentifierFeature>().First();
            var identifier = identifierFeature.GetIdentifier(codeDocument, codeDocument.Source);

            var metadataAttributeNode = new AttributesIntermediateNode(identifier);
            // Metadata attributes need to be inserted right before the class declaration.
            @namespace.Children.Insert(classIndex, metadataAttributeNode);
        }

        private sealed class AttributesIntermediateNode : ExtensionIntermediateNode
        {
            private const string CreateNewOnMetadataUpdateAttributeName = "global::System.Runtime.CompilerServices.CreateNewOnMetadataUpdateAttribute";
            private const string RazorFileIdentifierAttribute = "global::Microsoft.AspNetCore.Razor.Hosting.RazorFileIdentifierAttribute";
            private readonly string _identifier;

            public AttributesIntermediateNode(string identifier)
            {
                _identifier = identifier;
            }

            public override IntermediateNodeCollection Children => IntermediateNodeCollection.ReadOnly;

            public override void Accept(IntermediateNodeVisitor visitor)
            {
                AcceptExtensionNode(this, visitor);
            }

            public override void WriteNode(CodeTarget target, CodeRenderingContext context)
            {
                // [global::System.Runtime.CompilerServices.CreateNewOnMetadataUpdateAttribute]
                context.CodeWriter
                    .Write("[")
                    .Write(CreateNewOnMetadataUpdateAttributeName)
                    .WriteLine("]");;

                // [global:Microsoft.AspNetCore.Razor.Hosting.RazorFileIdentifierAttribute("/Views/Home/Index.cshtml")]
                context.CodeWriter
                    .Write("[")
                    .Write(RazorFileIdentifierAttribute)
                    .Write("(@\"")
                    .Write(_identifier)
                    .WriteLine("\")]");
            }
        }
    }
}
