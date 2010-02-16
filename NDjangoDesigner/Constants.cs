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

using System.Windows.Media;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace NDjango.Designer
{
    static class Constants
    {
        /// <summary>
        /// Classifier definition for selected tags 
        /// </summary>
        internal const string DJANGO_SELECTED_TAG = "ndjango.selected.tag";
        [Export]
        [Name(DJANGO_SELECTED_TAG)]
        private static ClassificationTypeDefinition DjangoSelectedTag;

        [Export(typeof(EditorFormatDefinition))]
        [Name("ndjango.selected.tag.format")]
        [DisplayName("NDjango Selected Tag Format")]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = DJANGO_SELECTED_TAG)]
        [Order]
        internal sealed class NDjangoSelectedTagFormat : ClassificationFormatDefinition
        {
            public NDjangoSelectedTagFormat()
            {
                BackgroundColor = Color.FromArgb(0xff, 0xee, 0xee, 0xee);//Colors.AliceBlue;
            }
        }

        internal const string DJANGO_SELECTED_TAGNAME = "ndjango.selected.tagname";
        [Export]
        [Name(DJANGO_SELECTED_TAGNAME)]
        private static ClassificationTypeDefinition DjangoSelectedTagName;

        [Export(typeof(EditorFormatDefinition))]
        [Name("ndjango.selected.tagname.format")]
        [DisplayName("NDjango Selected Tag Name Format")]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = DJANGO_SELECTED_TAGNAME)]
        [Order]
        internal sealed class NDjangoSelectedTagNameFormat : ClassificationFormatDefinition
        {
            public NDjangoSelectedTagNameFormat()
            {
                BackgroundColor = Colors.LightGray;
            }
        }

        internal const string MARKER_CLASSIFIER = "ndjango.marker";
        [Export]
        [Name(MARKER_CLASSIFIER)]
        internal static ClassificationTypeDefinition NDjangoMarker;

        [Export(typeof(EditorFormatDefinition))]
        [Name("ndjango.marker.format")]
        [DisplayName("ndjango marker format")]
        [UserVisible(false)]
        [ClassificationType(ClassificationTypeNames = MARKER_CLASSIFIER)]
        [Order]
        internal sealed class NDjangoMarkerFormat : ClassificationFormatDefinition
        {
            public NDjangoMarkerFormat()
            {
                BackgroundColor = Colors.Yellow;
            }
        }

        /// NDJANGO content type is defined to be just text - pretty much any text
        /// the actual filtering of the content types is done in the IsNDjango method 
        /// on the parser

        internal const string NDJANGO_TEXT = "plaintext";
        internal const string NDJANGO_HTML = "HTML";

        [Export]
        [Name("NDJANGO")]
        [BaseDefinition("plaintext")]
//        [BaseDefinition("HTML")]
        internal static ContentTypeDefinition TestContentTypeDefinition;
    }
}
