using Application.Interfaces;
using Domain.Common;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

public partial class CommentContentSanitizer : ICommentContentSanitizer
{
    private static readonly HashSet<string> AllowedTags = ["a", "code", "i", "strong"];

    // <  /?  name till closest >
    [GeneratedRegex(@"<(/?)([a-zA-Z][a-zA-Z0-9]*)([^<>]*)>")]
    private static partial Regex TagRegex();

    //  href="..."  title="..."  (title is optional)
    [GeneratedRegex(@"^\s+href=""([^""]*)""(?:\s+title=""([^""]*)"")?\s*$")]
    private static partial Regex LinkAttrsRegex();

    public Result<string> Sanitize(string input)
    {
        var result = new StringBuilder();
        var openTags = new Stack<string>();
        var pos = 0;

        foreach (Match m in TagRegex().Matches(input))
        {
            result.Append(WebUtility.HtmlEncode(input[pos..m.Index]));
            pos = m.Index + m.Length;

            var isClosing = m.Groups[1].Length > 0;
            var name = m.Groups[2].Value.ToLowerInvariant();
            var rawAttrs = m.Groups[3].Value;

            if (!AllowedTags.Contains(name))
            {
                return Result.Failure<string>(Error.Validation("Content.ValidatonError",$"Tag <{name}> is not allowed"));
            }

            if (isClosing)
            {
                if (!string.IsNullOrWhiteSpace(rawAttrs))
                {
                    return Result.Failure<string>(Error.Validation("Content.ValidatonError", $"Closing tag </{name}> cannot have attributes"));
                }

                if (!openTags.TryPop(out var opened) || opened != name)
                {
                   return Result.Failure<string>(Error.Validation("Content.ValidatonError", $"Unexpected closing tag </{name}>"));
                }
 
                result.Append($"</{name}>");
                continue;
            }

            var openTag = BuildOpenTag(name, rawAttrs);
            if (openTag.IsFailure)
            {
                return openTag;
            }

            openTags.Push(name);
            result.Append(openTag.Value);
        }

        result.Append(WebUtility.HtmlEncode(input[pos..]));

        if (openTags.Count > 0)
        {
            return Result.Failure<string>(Error.Validation("Content.ValidatonError", $"Tag <{openTags.Peek()}> is not closed"));
        }

        return Result.Success(result.ToString());
    }

    private static Result<string> BuildOpenTag(string name, string rawAttrs)
    {
        if (name == "a")
        {
            return BuildLink(rawAttrs);
        }

        if (!string.IsNullOrWhiteSpace(rawAttrs))
        {
            return Result.Failure<string>(Error.Validation("Content.ValidatonError", $"Tag <{name}> cannot have attributes"));
        }
            
        return Result.Success($"<{name}>");
    }

    private static Result<string> BuildLink(string rawAttrs)
    {
        var m = LinkAttrsRegex().Match(rawAttrs);
        if (!m.Success)
        {
            return Result.Failure<string>(Error.Validation("Content.ValidatonError", "Link must look like <a href=\"...\" title=\"...\">"));
        }

        var href = m.Groups[1].Value;
        if (!Uri.TryCreate(href, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return Result.Failure<string>(Error.Validation("Content.ValidatonError", "Link href must be a valid http/https URL"));
        }

        var safeHref = WebUtility.HtmlEncode(href);

        return Result.Success(m.Groups[2].Success
            ? $"<a href=\"{safeHref}\" title=\"{WebUtility.HtmlEncode(m.Groups[2].Value)}\">"
            : $"<a href=\"{safeHref}\">");
    }
}