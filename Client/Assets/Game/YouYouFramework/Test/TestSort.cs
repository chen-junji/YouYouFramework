using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSort : MonoBehaviour
{
	void Start()
	{
		int[] arr = new int[] { 5, 3, 4, 4, 1, 6, 7 };
		QuickSort(arr, 0, arr.Length - 1);
		Debug.LogError(arr.ToJson());
	}

	#region ц╟ещеепР
	void BubbleSort(int[] arr)
	{
		for (int i = 0; i < arr.Length - 1; i++)
		{
			for (int j = 0; j < arr.Length - 1 - i; j++)
			{
				if (arr[j] > arr[j + 1])
				{
					int temp = arr[j];
					arr[j] = arr[j + 1];
					arr[j + 1] = temp;
				}
			}
		}
	}
	#endregion

	#region ©ЛкыеепР
	void QuickSort(int[] arr, int low, int high)
	{
		int left = low;
		int right = high;
		if (left < right)
		{
			int key = arr[low];
			while (left < right)
			{
				while (left < right && arr[right] >= key) right--;
				arr[left] = arr[right];

				while (left < right && arr[left] <= key) left++;
				arr[right] = arr[left];
			}
			arr[left] = key;
			QuickSort(arr, low, left - 1);
			QuickSort(arr, left + 1, high);
		}
	}
	#endregion
}
