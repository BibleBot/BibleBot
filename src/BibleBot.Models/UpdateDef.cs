/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace BibleBot.Models
{
    /// <summary>
    /// A wrapper for EF Core ExecuteUpdate property assignments.
    /// </summary>
    public record UpdateDef<T>(Action<UpdateSettersBuilder<T>> Action) where T : class
    {
        /// <summary>
        /// Allows passing UpdateDef&lt;T&gt; directly to ExecuteUpdateAsync
        /// </summary>
        public static implicit operator Action<UpdateSettersBuilder<T>>(UpdateDef<T> def) => def.Action;

        /// <summary>
        /// Allows creating an UpdateDef&lt;T&gt; from a raw action
        /// </summary>
        public static implicit operator UpdateDef<T>(Action<UpdateSettersBuilder<T>> action) => new(action);

        /// <summary>
        /// Set a property to a value.
        /// Note: The value itself can still be an expression (e.g. u => u.Count + 1).
        /// </summary>
        public static UpdateDef<T> Set<TProperty>(Expression<Func<T, TProperty>> propertyExpression, TProperty value)
        {
            return new UpdateDef<T>(s => s.SetProperty(propertyExpression, value));
        }

        /// <summary>
        /// Combine multiple UpdateDef&lt;T&gt; into a single definition.
        /// </summary>
        public static UpdateDef<T> Combine(params UpdateDef<T>[] updates) => Combine((IEnumerable<UpdateDef<T>>)updates);

        /// <summary>
        /// Combine multiple UpdateDef&lt;T&gt; into a single definition by chaining the actions.
        /// </summary>
        public static UpdateDef<T> Combine(IEnumerable<UpdateDef<T>> updates)
        {
            return new UpdateDef<T>(s =>
            {
                foreach (var update in updates)
                {
                    update.Action(s);
                }
            });
        }
    }

    /// <summary>
    /// Extension methods for UpdateDef&lt;T&gt;.
    /// </summary>
    public static class UpdateDefExtensions
    {
        /// <summary>
        /// Combine multiple UpdateDef&lt;T&gt; into a single definition.
        /// </summary>
        public static UpdateDef<T> Combine<T>(this IEnumerable<UpdateDef<T>> updates) where T : class
            => UpdateDef<T>.Combine(updates);
    }
}
