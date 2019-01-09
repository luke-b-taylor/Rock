// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.BulkExport
{
    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude( "Export of Person record Attributes from ~/api/People/Export" )]
    public class AttributesExport
    {
        /// <summary>
        /// A Dictionary of AttributeKey and AttributeValue <see cref="AttributeReturnType"/>
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        [DataMember]
        public Dictionary<string, object> AttributeValues { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum AttributeReturnType
    {
        /// <summary>
        /// The raw attribute value
        /// </summary>
        Raw,

        /// <summary>
        /// The formatted attribute value
        /// </summary>
        Formatted
    }
}
