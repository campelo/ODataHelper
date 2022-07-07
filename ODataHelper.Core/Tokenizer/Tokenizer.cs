﻿namespace StringToExpression.Tokenizer
{
    /// <summary>
    /// Converts a string into a stream of tokens
    /// </summary>
    public class Tokenizer
    {
        /// <summary>
        /// Configuration of the tokens.
        /// </summary>
        public readonly IReadOnlyList<GrammerDefinition> GrammerDefinitions;

        /// <summary>
        /// Regex to identify tokens.
        /// </summary>
        protected readonly Regex TokenRegex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer"/> class.
        /// </summary>
        /// <param name="grammerDefinitions">The configuration for this language.</param>
        /// <exception cref="GrammerDefinitionDuplicateNameException">Thrown when two definitions have the same name.</exception>
        public Tokenizer(params GrammerDefinition[] grammerDefinitions)
        {
            //throw if we have any duplicates
            var duplicateKey = grammerDefinitions.GroupBy(x => x.Name).FirstOrDefault(g => g.Count() > 1)?.Key;
            if (duplicateKey != null)
                throw new GrammerDefinitionDuplicateNameException(duplicateKey);

            GrammerDefinitions = grammerDefinitions.ToList();

            var pattern = string.Join("|", GrammerDefinitions.Select(x => $"(?<{x.Name}>{x.Regex})"));
            this.TokenRegex = new Regex(pattern);
        }

        /// <summary>
        /// Convert text into a stream of tokens.
        /// </summary>
        /// <param name="text">text to tokenize.</param>
        /// <returns>stream of tokens.</returns>
        public IEnumerable<Token> Tokenize(string text)
        {
            var matches = TokenRegex.Matches(text).OfType<Match>();

            var expectedIndex = 0;
            foreach (var match in matches)
            {
                if (match.Index > expectedIndex)
                    throw new GrammerUnknownException(new StringSegment(text, expectedIndex, match.Index - expectedIndex));
                expectedIndex = match.Index + match.Length;

                var matchedTokenDefinition = GrammerDefinitions.FirstOrDefault(x => match.Groups[x.Name].Success);
                if (matchedTokenDefinition.Ignore)
                    continue;

                yield return new Token(
                    definition: matchedTokenDefinition,
                    value: match.Value,
                    sourceMap: new StringSegment(text, match.Index, match.Length));
            };

        }
    }
}
