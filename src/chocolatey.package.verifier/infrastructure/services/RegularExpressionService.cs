// Copyright � 2015 - Present RealDimensions Software, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// 	http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.package.verifier.Infrastructure.Services
{
    using System.Text.RegularExpressions;

    /// <summary>
    ///   Regular Expressions helper
    /// </summary>
    public class RegularExpressionService : IRegularExpressionService
    {
        /// <summary>
        ///   Replaces the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="matchEvaluator">The match evaluator.</param>
        /// <returns></returns>
        public string Replace(string input, string pattern, MatchEvaluator matchEvaluator)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return regex.Replace(input, matchEvaluator);
        }
    }
}