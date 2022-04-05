using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Logging;

namespace Wobble.Graphics.Sprites.Text.Formatting
{
    public class TextFormatter
    {
        /// <summary>
        ///     Ordered array of all text formatters by priority.
        /// </summary>
        public IFormatter[] TextFormatters { get; } =
        {
            new LinkFormatter(),
        };

        public Dictionary<System.Type, IRenderer> TextRenderers { get; } = new Dictionary<Type, IRenderer>();

        /// <summary>
        ///     Default renderers to be used by the text formatter. PlainTextRenderer is always assumed.
        /// </summary>
        private static readonly IRenderer[] defaultRenderers =
        {
            new LinkRenderer(),
        };

        /// <summary>
        ///     Creates a new text formatter.
        /// </summary>
        /// <param name="formatters">Array of custom formatters.</param>
        /// <param name="renderers">Array of custom renderers. PlainTextRenderer is assumed, and does not need to be included.</param>
        public TextFormatter(IFormatter[] formatters = null, IRenderer[] renderers = null)
        {
            if (formatters != null)
                TextFormatters = formatters;

            var textRenderers = renderers ?? defaultRenderers;
            
            for(int i = 0; i < textRenderers.Length; i++)
                if(!TextRenderers.TryAdd(textRenderers[0].FragmentType(), textRenderers[0]))
                    Logger.Warning($"Text Renderer {textRenderers[0].GetType()} cannot be initialised, as there is already another Renderer for the {textRenderers[0].FragmentType()} Fragment.", LogType.Runtime);
        }

        public LinkedList<TextFragment> Format(string rawText)
        {
            LinkedList<TextFragment> fragments = new LinkedList<TextFragment>();

            fragments.AddFirst(new UnparsedTextFragment(rawText));

            FormatFragments(fragments, TextFormatters);

            return fragments;
        }
        
        private void FormatFragments(LinkedList<TextFragment> sourceFragments, IFormatter[] availableFormatters)
        {
            bool isComplete = true;
            var currentFragment = sourceFragments?.First;

            while (!(currentFragment == null && isComplete))
            {
                // Single, unparsed fragment.
                if (currentFragment.Value is UnparsedTextFragment unparsed)
                {
                    // There still exists unparsed text.
                    isComplete = false;

                    bool wasParsed = false;

                    for (int i = 0; i < availableFormatters.Length; i++)
                    {
                        var formatter = availableFormatters[i];

                        var result = formatter.FormatText(unparsed.RawText);

                        if (result != null)
                        {
                            // Successfully formatted this text block.
                            wasParsed = true;

                            // Update fragments in list.
                            LinkedListNode<TextFragment> lastFragment = null;
                            for (int j = 0; j < result.Length; j++)
                                lastFragment = sourceFragments.AddBefore(currentFragment, result[j]);

                            // Current fragment has been replaced, remove from list.
                            sourceFragments.Remove(currentFragment);

                            currentFragment = lastFragment;

                            // Move onto the next block, stop parsing the current one.
                            break;
                        }
                    }

                    // If this text was not parsed by any parsers, convert to a PlainTextFragment.
                    if (!wasParsed)
                        currentFragment.Value = new PlainTextFragment(unparsed.RawText);
                }
                // Check nested elements
                else if (currentFragment.Value.Inner.Count > 0)
                {
                    var formatters = availableFormatters.Where(x => x.FragmentType() != currentFragment.Value.GetType()).ToArray();
                    
                    FormatFragments(currentFragment.Value.Inner, formatters);
                }

                currentFragment = currentFragment.Next;

                // If we have reached the end of the list,
                // but we are not yet complete, loop again.
                if (currentFragment == null && !isComplete)
                {
                    currentFragment = sourceFragments.First;

                    // Temporary change, will be reverted when at unparsed fragment.
                    isComplete = true;
                }
            }
        }
        
        /// <summary>
        ///     Called when the text component is about to be drawn.
        ///     Wraps the text and creates sprites.
        /// </summary>
        public void CreateSprites(SpriteTextPlus parent, LinkedList<TextFragment> fragments)
        {
            var current = fragments.First;

            float width = 0f, height = 0f;

            float currentLineWidth = 0f;
            int newLineCount = 0;

            List<SpriteTextPlusLineRaw> currentLineSprites = new List<SpriteTextPlusLineRaw>();

            while (current != null)
            {
                var builder = new StringBuilder();
                GetTextRecursive(builder, current.Value);

                var text = builder.ToString();

                string line = text;

                var newLineIndex = text.IndexOf('\n');
                if (newLineIndex == 0)
                {
                    // Empty line, skip
                    int _splitIndex = 0;
                    Split(current, ref _splitIndex);
                    
                    parent.Font.Store.Size = parent.FontSize;
                    height += parent.Font.Store.GetLineHeight();

                    currentLineWidth = 0f;

                    current = current.Next;
                    continue;
                }
                
                if (newLineIndex != -1)
                {
                    // Split fragments at this position.
                    // Copy index due to ref.
                    // ref is required for recursion.
                    int _splitIndex = newLineIndex;
                    Split(current, ref _splitIndex);
                    
                    // Use substring instead of GetTextRecursive for efficiency
                    line = text.Substring(0, newLineIndex);

                    newLineCount++;
                }

                var sprites = CreateSpritesRecursive(parent, current.Value);
                var spriteWidths = sprites.Select(x => x.Width).ToList();
                var textLengths = sprites.Select(x => x.Text.Length).ToList();
                var totalFragmentWidth = spriteWidths.Sum();
                
                // -- Insert YaLTeR Code --
                if (parent.MaxWidth != null && currentLineWidth + totalFragmentWidth > parent.MaxWidth)
                {
                    Debug.Assert(line.Length > 0);

                    // Try to split the line on spaces to fit it into MaxWidth.
                    var spaces = new List<int>();
                    for (var i = 0; i < line.Length; i++)
                    {
                        if (char.IsWhiteSpace(line[i]))
                            spaces.Add(i);
                    }

                    // Binary search would be great for the next two (as long as we're assuming that
                    // more characters == longer lines, which will not hold for complex scripts,
                    // which aren't supported yet anyway), but C# doesn't have a built-in method
                    // for binary search by an arbitrary predicate. So I guess we'll just go with a regular find-last,
                    // which can be slower, but has a bonus of not making any of the aforementioned assumptions.
                    var splitOnIndex = spaces.FindLastIndex(spacePosition =>
                    {
                        var relativeSpacePosition = spacePosition;
                        float previousWidth = 0;

                        int spriteIndex = 0;
                        for (int i = 0; i < textLengths.Count; i++)
                        {
                            if (relativeSpacePosition > textLengths[i])
                            {
                                // Space not in current sprite.
                                // Adjust relative index.
                                relativeSpacePosition -= textLengths[i];
                                
                                // Add previous sprite width
                                previousWidth += spriteWidths[i];
                                continue;
                            }

                            // Space is in this sprite.
                            spriteIndex = i;
                            break;
                        }
                        
                        var sprite = sprites[spriteIndex];
                        
                        sprite.Text = sprite.Text.Substring(0, relativeSpacePosition);

                        var spriteWidth = currentLineWidth + previousWidth + sprite.Width;

                        if (spriteWidth <= parent.MaxWidth)
                        {
                            // Clean up remaining sprites, will be recreated next iteration.
                            for(int j = spriteIndex + 1; j < sprites.Count; j++)
                                sprites[j].Destroy();

                            return true;
                        }

                        return false;
                    });

                    int fragmentSplitIndex = 0;
                    bool skipChar = true;
                    
                    if (splitOnIndex == -1)
                    {
                        // Splitting even on the first whitespace gives a line that's too long (or there are no spaces at all),
                        // so split on any character.
                        //
                        // Splitting on arbitrary characters like this is questionable, because while even in English
                        // splitting mid-word can produce nonsensical results, in some complex scripts it doesn't
                        // make any sense whatsoever to split on something that's not a space. But, once again,
                        // we don't support complex scripts yet, and this is a decent enough fallback to make sure
                        // the lines don't get too off max width.
                        var lastIndex = line.Length;
                        if (spaces.Count > 0)
                            lastIndex = spaces[0];
                        
                        for (var i = lastIndex; i != 0; i--)
                        {
                            var relativeCharPosition = i;
                            float previousWidth = 0;

                            // This follows the same logic as the space detection up above.
                            // It is necessary to get the widths of the previous sprites and hence
                            // follow the same logic as the spaces, even though this is sequential (and descending).
                            int spriteIndex = 0;
                            for (int j = 0; j < textLengths.Count; j++)
                            {
                                if (relativeCharPosition > textLengths[j])
                                {
                                    // Target char not in current sprite.
                                    // Adjust relative index.
                                    relativeCharPosition -= textLengths[j];
                                
                                    // Add previous sprite width
                                    previousWidth += spriteWidths[j];
                                    continue;
                                }

                                // Target char is in this sprite.
                                spriteIndex = j;
                                break;
                            }

                            var sprite = sprites[spriteIndex];
                            
                            sprite.Text = sprite.Text.Substring(0, relativeCharPosition);

                            var spriteWidth = currentLineWidth + previousWidth + sprite.Width;

                            // If we're left with 1 character, just go with it even if we're over MaxWidth.
                            if (spriteWidth > parent.MaxWidth && i > 1)
                                continue;

                            // Clean up remaining sprites, will be recreated next iteration.
                            for(int j = spriteIndex + 1; j < sprites.Count; j++)
                                sprites[j].Destroy();

                            fragmentSplitIndex = i;
                            skipChar = false; // Do not skip the character, as it is not whitespace.
                            break;
                        }
                    }
                    else
                    {
                        // Split fragment at new line
                        fragmentSplitIndex = spaces[splitOnIndex];
                    }

                    Split(current, ref fragmentSplitIndex, skipChar);
                    
                    newLineCount++;
                }
                
                float remainderWidth = 0f;
                for (int i = 0; i < sprites.Count; i++)
                {
                    var sprite = sprites[i];

                    // Move to correct X position, relative to the start of the line.
                    sprite.X += currentLineWidth + remainderWidth;

                    sprite.UsePreviousSpriteBatchOptions = true;

                    remainderWidth += sprite.Width;
                }
                
                currentLineWidth += remainderWidth;

                // Add new sprites to the line
                currentLineSprites.AddRange(sprites);
                
                if (newLineCount > 0)
                {
                    // Create a new line
                    var lineSprite = new SpriteTextPlusLine(currentLineSprites.ToArray())
                    {
                        Parent = parent,
                        Y = height,
                        Alignment = parent.ConvertTextAlignment(),
                        UsePreviousSpriteBatchOptions = true
                    };
                    
                    width = Math.Max(width, lineSprite.Width);
                    
                    // Update current height/Y position
                    parent.Font.Store.Size = parent.FontSize;
                    height += parent.Font.Store.GetLineHeight();

                    // Reset line variables
                    currentLineSprites.Clear();
                    currentLineWidth = 0f;
                    newLineCount--;
                }
                
                current = current.Next;
            }
            
            // Create final line
            var finalLineSprite = new SpriteTextPlusLine(currentLineSprites.ToArray())
            {
                Parent = parent,
                Y = height,
                Alignment = parent.ConvertTextAlignment(),
                UsePreviousSpriteBatchOptions = true
            };
            
            width = Math.Max(width, finalLineSprite.Width);
            
            // Add on height of last line
            parent.Font.Store.Size = parent.FontSize;
            height += parent.Font.Store.GetLineHeight();
            
            parent.Size = new ScalableVector2(width, height);
            
            // todo: OnTextUpdate
        }

        private LinkedListNode<TextFragment> Split(LinkedList<TextFragment> fragments, ref int splitIndex, bool skipSplitChar = true)
        {
            var current = fragments.First;
            while (current != null)
            {
                var result = Split(current, ref splitIndex, skipSplitChar); // Calls overload below

                if (result != null)
                    return result;
                
                current = current.Next;
            }

            return null;
        }
        
        private LinkedListNode<TextFragment> Split(LinkedListNode<TextFragment> fragment, ref int splitIndex, bool skipSplitChar = true)
        {
            if (fragment.Value is PlainTextFragment text)
            {
                if (text.DisplayText.Length > splitIndex)
                {
                    // Split point is in this fragment.

                    var left = text.DisplayText.Substring(0, splitIndex);
                    var right = text.DisplayText.Substring(skipSplitChar ? splitIndex + 1 : splitIndex);

                    text.DisplayText = left;

                    // Create a new fragment if the right part of the split is not empty.
                    if (right != "")
                    {
                        var newFragment = new PlainTextFragment(right);
                        return fragment.List.AddAfter(fragment, newFragment);
                    }
                    
                    // Default to next item.
                    return fragment.Next;
                }
                    
                // Split point is not in this fragment.
                // Adjust index so it becomes relative to the next PlainText instance.
                splitIndex -= text.DisplayText.Length;
            }
            else if (fragment.Value.Inner.Count > 0)
            {
                var remainder = Split(fragment.Value.Inner, ref splitIndex, skipSplitChar); // Calls overload above

                var newParent = (TextFragment)fragment.Value.Clone();
                newParent.Inner = new LinkedList<TextFragment>();

                // Move remaining fragments to a new parent
                while (remainder != null)
                {
                    newParent.Inner.AddLast(remainder.Value);
                    fragment.Value.Inner.Remove(remainder.Value);
                        
                    remainder = remainder.Next;
                }

                return fragment.Next;
            }

            return null;
        }

        private void GetTextRecursive(StringBuilder builder, LinkedList<TextFragment> fragments)
        {
            var current = fragments.First;
            while (current != null)
            {
                // Calls overload below
                GetTextRecursive(builder, current.Value);

                current = current.Next;
            }
        }

        private void GetTextRecursive(StringBuilder builder, TextFragment fragment)
        {
            if (fragment is PlainTextFragment text)
                builder.Append(text.DisplayText);
            else if(fragment.Inner.Count > 0)
                // Calls overload above
                GetTextRecursive(builder, fragment.Inner);
        }

        private List<SpriteTextPlusLineRaw> CreateSpritesRecursive(SpriteTextPlus parent, TextFragment fragment)
        {
            if (fragment is PlainTextFragment f)
            {
                // Modify this variable, then add to list
                SpriteTextPlusLineRaw sprite = new SpriteTextPlusLineRaw(parent.Font, f.DisplayText, parent.FontSize)
                {
                    Tint = parent.Tint,
                    Alpha = parent.Alpha,
                    SpriteBatchOptions = new SpriteBatchOptions
                    {
                        DoNotScale = true,
                        BlendState = BlendState.AlphaBlend
                    }
                };

                return new List<SpriteTextPlusLineRaw>() { sprite };
            }

            IRenderer renderer;
            if (!TextRenderers.TryGetValue(fragment.GetType(), out renderer))
            {
                Logger.Warning($"No corresponding Text Renderer found for Fragment type {fragment.GetType()}.", LogType.Runtime);

                return new List<SpriteTextPlusLineRaw>();
            }

            var newSprites = new List<SpriteTextPlusLineRaw>();
            
            var innerFragment = fragment.Inner.First;
            
            while (innerFragment != null)
            {
                var innerSprites = CreateSpritesRecursive(parent, innerFragment.Value);

                for(int i = 0; i < innerSprites.Count; i++)
                {
                    var inner = innerSprites[i];
                    
                    // Apply current renderer and add to the list
                    renderer.ModifySprite(fragment, inner);

                    newSprites.Add(inner);
                }
    
                innerFragment = innerFragment.Next;
            }
            
            return newSprites;
        }
    }
}