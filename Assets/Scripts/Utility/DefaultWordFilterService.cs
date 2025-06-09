using UnityEngine;
using System.Collections.Generic;
using QFramework;

using System.Collections.Generic;
using System.Text;

public interface IWordFilterService : IUtility
{
    string Filter(string input);
    void ReloadWords(List<string> words);
}

public class DefaultWordFilterService : IWordFilterService
{
    private HashSet<string> mBannedWords = new HashSet<string>();
    
    public string Filter(string input)
    {
        if (string.IsNullOrEmpty(input) || mBannedWords.Count == 0)
            return input;

        StringBuilder result = new StringBuilder(input);
        
        foreach (var word in mBannedWords)
        {
            if (!string.IsNullOrEmpty(word))
            {
                result.Replace(word, new string('*', word.Length));
            }
        }
        
        return result.ToString();
    }
    
    public void ReloadWords(List<string> words)
    {
        mBannedWords.Clear();
        mBannedWords.UnionWith(words);
    }
}