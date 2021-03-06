﻿namespace Aehnlich.ViewModels.Documents
{
	using Aehnlich.Interfaces;
	using System.Windows.Input;

	/// <summary>
	/// Defines common properties and methods for document viewmodels.
	/// </summary>
	internal abstract class DocumentBaseViewModel : PaneViewModel, IDocumentBaseViewModel
	{
		#region fields
		private bool _IsFilePathReal;
		private string _FilePath;
		private bool _IsDirty;
		#endregion fields

		#region ctors
		/// <summary>
		/// Class constructor
		/// </summary>
		protected DocumentBaseViewModel()
		{
		}
		#endregion ctors

		#region properties
		/// <summary>
		/// Get whether the current path does exist on disk or not. This is for example useful
		/// when we have done File>New but not, yet, File>Save. In this case, the document may
		/// have name (Untitled.txt) but System.IO.File.Exists is likely to be useless in this situation.'
		/// </summary>
		public bool IsFilePathReal
		{
			get { return _IsFilePathReal; }
			protected set
			{
				if (_IsFilePathReal != value)
				{
					_IsFilePathReal = value;
					NotifyPropertyChanged(() => IsFilePathReal);
				}
			}
		}

		/// <summary>
		/// Gets the string that determines where the document is saved
		/// if it can be saved and has been save/loaded before.
		/// 
		/// <seealso cref="IsFilePathReal"/>
		/// </summary>
		public string FilePath
		{
			get { return _FilePath; }
			protected set
			{
				if (_FilePath != value)
				{
					_FilePath = value;
					NotifyPropertyChanged(() => FilePath);
				}
			}
		}

		/// <summary>This property decides whether the document needs to be saved or not.</summary>
		public bool IsDirty
		{
			get { return _IsDirty; }
			protected set
			{
				if (_IsDirty != value)
				{
					_IsDirty = value;
					NotifyPropertyChanged(() => IsDirty);
				}
			}
		}

		/// <summary>Gets a command that is invoked when the user clicks the document tab close button.</summary>
		abstract public ICommand CloseCommand { get; }

		/// <summary>Gets a command that is invoked when a document is saved. Returning null here should disable
		/// controls that bind to this command, which is just what we need for the Start Page.</summary>
		abstract public ICommand SaveCommand { get; }
		#endregion properties

		#region methods
		/// <summary>
		/// When Overwritten - This method is invoked before application shut down
		/// to save all relevant user settings for recovery on appliaction re-start.
		/// </summary>
		abstract public void SaveSettings();

		/// <summary>
		/// Gets a title for display in the document tab.
		/// </summary>
		/// <param name="leftPath"></param>
		/// <param name="rightPath"></param>
		/// <param name="isDirty"></param>
		/// <returns></returns>
		protected virtual string GetTitle(string leftFilePath, string rightFilePath, bool isDirty)
		{
			string leftDirName = GetDirName(leftFilePath);
			string rightDirName = GetDirName(rightFilePath);

			if (string.Compare(leftDirName, rightDirName, true) == 0)
				return leftDirName + (isDirty ? "*" : string.Empty);

			return leftDirName + " ~ " + rightDirName + (isDirty ? "*" : string.Empty);
		}

		/// <summary>
		/// Gets the name of a directory of a given path
		/// path = 'c:\tmp\my\subdir' returns 'subdir'
		/// 
		/// or the path itself if there is no subdir to return:
		/// path = 'c:\' returns 'c:\'
		/// 
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		private string GetDirName(string filePath)
		{
			string dirName = string.Empty;

			try
			{
				if (string.IsNullOrEmpty(filePath) == false)
				{
					dirName = filePath.Trim(System.IO.Path.DirectorySeparatorChar);
					dirName = System.IO.Path.GetFileName(dirName);

					if (string.IsNullOrEmpty(dirName) == true)
						dirName = filePath;
				}
			}
			catch
			{
				// System.IO likes to throw things on wrong strings
				// so we go without the name if things go wrong...
				dirName = filePath;
			}

			return dirName;
		}

		/// <summary>
		/// Gets a tool tip for display on mouseover over the document tab.
		/// </summary>
		/// <param name="leftPath"></param>
		/// <param name="rightPath"></param>
		/// <returns></returns>
		protected virtual string GetTooltip(string leftPath, string rightPath)
		{
			return leftPath + "\n" +
				   rightPath;
		}
		#endregion methods
	}
}
