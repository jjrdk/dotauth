// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace DotAuth.Extensions;

using Microsoft.AspNetCore.Routing;

/// <summary>
/// Defines the action information.
/// </summary>
internal sealed class ActionInformation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActionInformation"/> class.
    /// </summary>
    /// <param name="controllerName">Name of the controller.</param>
    /// <param name="actionName">Name of the action.</param>
    /// <param name="area">The area.</param>
    public ActionInformation(string controllerName, string actionName, string? area = null)
    {
        ControllerName = controllerName;
        ActionName = actionName;
        Area = area;
    }

    /// <summary>
    /// Gets the name of the controller.
    /// </summary>
    /// <value>
    /// The name of the controller.
    /// </value>
    public string ControllerName { get; }

    /// <summary>
    /// Gets the name of the action.
    /// </summary>
    /// <value>
    /// The name of the action.
    /// </value>
    public string ActionName { get; }

    /// <summary>
    /// Gets the area.
    /// </summary>
    /// <value>
    /// The area.
    /// </value>
    public string? Area { get; }

    /// <summary>
    /// Gets or sets the route value dictionary.
    /// </summary>
    /// <value>
    /// The route value dictionary.
    /// </value>
    public RouteValueDictionary RouteValueDictionary { get; set; } = new();
}