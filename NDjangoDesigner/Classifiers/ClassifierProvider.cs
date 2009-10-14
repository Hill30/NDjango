/****************************************************************************
 * 
 *  NDjango Parser Copyright © 2009 Hill30 Inc
 *
 *  This file is part of the NDjango Designer.
 *
 *  The NDjango Parser is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  The NDjango Parser is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with NDjango Parser.  If not, see <http://www.gnu.org/licenses/>.
 *  
 ***************************************************************************/

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.ApplicationModel.Environments;
using NDjango.Designer.Parsing;

namespace NDjango.Designer.Classifiers
{
    /// <summary>
    /// Provides classifiers for TextBuffers
    /// </summary>
    /// <remarks>Imports the Parser object and passes it on to the new classifiers so that 
    /// classifiers can generate tokenzers</remarks>
    [Export(typeof(IClassifierProvider))]
    [ContentType(Constants.NDJANGO)]
    [Name("NDjango Classifier")]
    internal class ClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService classificationTypeRegistry { get; set; }

        [Import]
        internal INodeProviderBroker nodeProviderBroker { get; set; }

        /// <summary>
        /// Providers classifers for NDjango buffers
        /// </summary>
        /// <param name="textBuffer"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public IClassifier GetClassifier(ITextBuffer textBuffer, IEnvironment context)
        {
            if (nodeProviderBroker.IsNDjango(textBuffer, context))
                return new Classifier(nodeProviderBroker, classificationTypeRegistry, textBuffer);
            else
                return null;
        }
    }
}
