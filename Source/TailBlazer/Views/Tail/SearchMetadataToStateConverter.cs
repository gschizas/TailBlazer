using System;
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using DynamicData.Kernel;
using TailBlazer.Domain.Settings;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Tail
{
    public class SearchMetadataToStateConverter : IConverter<SearchState[]>
    {
        private static class Structure
        {
            public const string Root = "TailView";

            public const string SearchList = "SearchList";
            public const string SearchItem = "SearchItem";

            public const string Text = "Text";
            public const string Filter = "Filter";
            public const string UseRegEx = "UseRegEx";
            public const string Highlight = "Highlight";
            public const string Alert = "Alert";
            public const string IgnoreCase = "IgnoreCase";

            public const string Swatch = "Swatch";
            public const string Hue = "Hue";
            public const string Icon = "Icon";
        }

        public SearchState[] Convert(State state)
        {
            var doc = XDocument.Parse(state.Value);
            var root = doc.ElementOrThrow(Structure.Root);
            var searchStates = Convert(root);
            return searchStates;
        }

        public SearchState[] Convert(XElement root)
        {
            return root.Element(Structure.SearchList)
                .Elements(Structure.SearchItem)
                .Select((element, index) =>
                {
                    var text = ((XElement)element).ElementOrThrow(Structure.Text);
                    var position = ParseEx.ParseInt(element.Attribute(Structure.Filter).Value).ValueOr(() => index);
                    var filter = element.Attribute(Structure.Filter).Value.ParseBool().ValueOr(() => true);
                    var useRegEx = element.Attribute(Structure.UseRegEx).Value.ParseBool().ValueOr(() => false);
                    var highlight = element.Attribute(Structure.Highlight).Value.ParseBool().ValueOr(() => true);
                    var alert = element.Attribute(Structure.Alert).Value.ParseBool().ValueOr(() => false);
                    var ignoreCase = element.Attribute(Structure.IgnoreCase).Value.ParseBool().ValueOr(() => true);

                    var swatch = element.Attribute(Structure.Swatch).Value;
                    var hue = element.Attribute(Structure.Hue).Value;
                    var icon = element.Attribute(Structure.Icon).Value;

                    return new SearchState(text, position, useRegEx, highlight, filter, alert, ignoreCase, swatch, icon, hue);
                }).ToArray();
        }

        public State Convert(SearchState[] state)
        {
            var list = new XElement(Structure.SearchList);

            var searchItemsArray = state.Select(f => new XElement(Structure.SearchItem,
                new XElement(Structure.Text, f.Text),
                new XAttribute(Structure.Filter, f.Filter),
                new XAttribute(Structure.UseRegEx, f.UseRegEx),
                new XAttribute(Structure.Highlight, f.Highlight),
                new XAttribute(Structure.Alert, f.Alert),
                new XAttribute(Structure.IgnoreCase, f.IgnoreCase),
                new XAttribute(Structure.Swatch, f.Swatch),
                new XAttribute(Structure.Hue, f.Hue),
                new XAttribute(Structure.Icon, f.Icon)));

            searchItemsArray.ForEach(list.Add);

            XDocument doc = new XDocument(list);
            return new State(1, doc.ToString());
        }

        public SearchState[] GetDefaultValue()
        {
            return Enumerable.Empty<SearchState>().ToArray();
        }
    }
}