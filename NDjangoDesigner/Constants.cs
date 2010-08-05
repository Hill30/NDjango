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
using System;

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
        [Name("NDjango Selected Tag")]
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
        [Name("NDjango Selected Tag Name")]
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
        internal static ClassificationTypeDefinition NDjangoMarker = null; // null is not really necessary, but to keep the compiler happy...

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

        /// <summary>
        /// Package constants
        /// </summary>

        public const string guidDesignerPkgString = "40154ca2-08a7-4440-89fb-e4b78ead53d9";
        public const string guidDesignerCmdSetString = "e3233f52-718c-4dc5-a792-d92f77850b8b";

        public static readonly Guid guidDesignerCmdSet = new Guid(guidDesignerCmdSetString);

        public const uint cmdidAddView = 0x100;

        public const string guidNDjangoDesignerPkgString = "2780818b-02fb-4990-a051-7cee3fd09157";
        public const string guidNDjangoDesignerCmdSetString = "7686279e-0421-4c87-8ed3-a484a22b58f3";
        public const string UICONTEXT_ViewsSelectedString = "5ebe12b1-2c8a-4e83-85d5-1f26eb36561c";
        public const uint cmdidNDjangoDesigner = 0x0101;
        public static readonly Guid guidNDjangoDesignerCmdSet = new Guid(guidNDjangoDesignerCmdSetString);
        public static readonly Guid UICONTEXT_ViewsSelected = new Guid(UICONTEXT_ViewsSelectedString);

    }
}
