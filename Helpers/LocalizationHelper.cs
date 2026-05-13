using System.Text.RegularExpressions;
using Terraria.Localization;

namespace DragonLens.Helpers
{
	internal static class LocalizationHelper
	{
		/// <summary>
		/// Gets a localized text value of the mod.
		/// If no localization is found, the key itself is returned.
		/// </summary>
		/// <param name="key">the localization key</param>
		/// <param name="args">optional args that should be passed</param>
		/// <returns>the text should be displayed</returns>
		public static string GetText(string key, params object[] args)
		{
			string value = Language.Exists($"Mods.DragonLens.{key}") ? Language.GetTextValue($"Mods.DragonLens.{key}", args) : key;
			return ApplyControlTokens(value);
		}

		public static string ApplyControlTokens(string value)
		{
			if (string.IsNullOrEmpty(value))
				return value;

			value = Regex.Replace(value, @"[ \t]*\bNEWBLOCK\b[ \t]*", "\n\n");
			return Regex.Replace(value, @"[ \t]*\bNEWLN\b[ \t]*", "\n");
		}

		public static string GetGUIText(string key, params object[] args)
		{
			return GetText($"GUI.{key}", args);
		}

		public static string GetToolText(string key, params object[] args)
		{
			return GetText($"Tools.{key}", args);
		}

		public static bool IsCjkPunctuation(char a)
		{
			return Regex.IsMatch(a.ToString(), @"\p{IsCJKSymbolsandPunctuation}|\p{IsHalfwidthandFullwidthForms}");
		}

		public static bool IsCjkUnifiedIdeographs(char a)
		{
			return Regex.IsMatch(a.ToString(), @"\p{IsCJKUnifiedIdeographs}");
		}

		public static bool IsRightCloseCjkPunctuation(char a)
		{
			return a is '（' or '【' or '《' or '｛' or '｢' or '［' or '｟' or '“';
		}

		public static bool IsCjkCharacter(char a)
		{
			return IsCjkUnifiedIdeographs(a) || IsCjkPunctuation(a);
		}
	}
}
