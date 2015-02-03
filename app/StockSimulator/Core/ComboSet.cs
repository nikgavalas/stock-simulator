using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	/// <summary>
	/// Makes all possible combinations of a list.
	/// </summary>
	/// <typeparam name="T">Type of the element in the array.</typeparam>
	public class ComboSet<T>
	{
		private List<T> mOrigList;
		private List<T> mCurrentCombo;
		private List<List<T>> mAllCombos;

		/// <summary>
		/// Save the list to be used later.
		/// </summary>
		/// <param name="list">List to find the combos in.</param>
		public ComboSet(List<T> list)
		{
			mOrigList = list;
			mCurrentCombo = new List<T>();
			mAllCombos = new List<List<T>>();
		}

		/// <summary>
		/// Returns a list of lists of all the possible combinations of the array
		/// ignoring duplicates. Ex. 1, 2, 3 is the same as 3, 2, 1.
		/// </summary>
		/// <param name="minComboSize">The minimum size of a combo</param>
		/// <returns>The list of combos</returns>
		public List<List<T>> GetSet(int minComboSize)
		{
			mAllCombos = new List<List<T>>();

			// Find all the combos of each amount. So first find all the combos
			// for just one element, then two, etc.
			for (int i = minComboSize - 1; i < mOrigList.Count; i++)
			{
				Find(0, i + 1);
			}

			return mAllCombos;
		}

		/// <summary>
		/// Recursive function that finds the combos in an array.
		/// </summary>
		/// <param name="offset">Offset for the recursive function to work</param>
		/// <param name="k">How long the combo should be</param>
		private void Find(int offset, int k)
		{
			if (k == 0)
			{
				SaveCombo(mCurrentCombo);
				return;
			}

			for (int i = offset; i <= mOrigList.Count - k; ++i)
			{
				mCurrentCombo.Add(mOrigList[i]);
				Find(i + 1, k - 1);
				mCurrentCombo.RemoveAt(mCurrentCombo.Count - 1);
			}

		}

		/// <summary>
		/// Saves a found combo in the big list.
		/// </summary>
		/// <param name="comboList">The current combo to be saved</param>
		private void SaveCombo(List<T> comboList)
		{
			mAllCombos.Add(new List<T>(comboList));
		}
	}
}
