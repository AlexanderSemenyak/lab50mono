// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.Mvc
{
    public class FilterAttributeFilterProvider : IFilterProvider
    {
        private readonly bool _cacheAttributeInstances;

        public FilterAttributeFilterProvider()
            : this(true)
        {
        }

        public FilterAttributeFilterProvider(bool cacheAttributeInstances)
        {
            _cacheAttributeInstances = cacheAttributeInstances;
        }

        protected virtual IEnumerable<FilterAttribute> GetActionAttributes(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            return actionDescriptor.GetFilterAttributes(_cacheAttributeInstances);
        }

        protected virtual IEnumerable<FilterAttribute> GetControllerAttributes(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            return actionDescriptor.ControllerDescriptor.GetFilterAttributes(_cacheAttributeInstances);
        }

        public virtual IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            ControllerBase controller = controllerContext.Controller;
            if (controller == null)
            {
                return Enumerable.Empty<Filter>();
            }

            var typeFilters = GetControllerAttributes(controllerContext, actionDescriptor)
                .Select(attr => new Filter(attr, FilterScope.Controller, null));
            var methodFilters = GetActionAttributes(controllerContext, actionDescriptor)
                .Select(attr => new Filter(attr, FilterScope.Action, null));

            return typeFilters.Concat(methodFilters).ToList();
        }
    }
}
