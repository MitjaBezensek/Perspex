// Copyright (c) The Perspex Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Collections;
using System.Reactive.Linq;
using Perspex.Controls;
using Perspex.Controls.Templates;
using Perspex.Markup.Data;
using Perspex.Markup.Xaml.Data;
using Perspex.Metadata;

namespace Perspex.Markup.Xaml.Templates
{
    public class TreeDataTemplate : ITreeDataTemplate
    {
        public Type DataType { get; set; }

        [Content]
        public TemplateContent Content { get; set; }

        public Binding ItemsSource { get; set; }

        public bool Match(object data)
        {
            if (DataType == null)
            {
                throw new InvalidOperationException("DataTemplate must have a DataType.");
            }

            return DataType == data.GetType();
        }

        public IEnumerable ItemsSelector(object item)
        {
            if (ItemsSource != null)
            {
                var obs = new ExpressionObserver(item, ItemsSource.Path);
                return obs.Take(1).Wait() as IEnumerable;
            }

            return null;
        }

        public bool IsExpanded(object item)
        {
            return true;
        }

        public IControl Build(object data)
        {
            var visualTreeForItem = Content.Load();
            visualTreeForItem.DataContext = data;
            return visualTreeForItem;
        }
    }
}