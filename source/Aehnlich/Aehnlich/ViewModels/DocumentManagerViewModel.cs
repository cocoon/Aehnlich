﻿namespace Aehnlich.ViewModels
{
    using Aehnlich.Events;
    using Aehnlich.Interfaces;
    using Aehnlich.ViewModels.Documents;
    using AehnlichDirViewModelLib.Events;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Threading;

    public class DocumentManagerViewModel : Base.ViewModelBase, IDocumentManagerViewModel
    {
        #region fields
        private readonly ObservableRangeCollection<IDocumentBaseViewModel> _Documents;
        private IDocumentBaseViewModel _ActiveDocument;
        private bool _Disposed;
        #endregion fields

        #region ctors
        /// <summary>
        /// Class constructor
        /// </summary>
        public DocumentManagerViewModel()
        {
            _Documents = new ObservableRangeCollection<IDocumentBaseViewModel>();
        }
        #endregion ctors

        #region events
        /// <summary>
        /// The document with the current input focus has changed when this event fires.
        /// </summary>
        public event DocumentChangedEventHandler ActiveDocumentChanged;
        #endregion event

        #region properties
        public IEnumerable<IDocumentBaseViewModel> Documents { get { return _Documents; } }

        /// <summary>
        /// Gets/sets the dcoument that is currently active (has input focus) - if any.
        /// </summary>
        public IDocumentBaseViewModel ActiveDocument
        {
            get { return _ActiveDocument; }

            set
            {
                if (_ActiveDocument != value)
                {
                    _ActiveDocument = value;

                    NotifyPropertyChanged(() => ActiveDocument);

                    // Ensure that no pending calls are in the dispatcher queue
                    // This makes sure that we are blocked until bindings are re-established
                    // (Bindings are, for example, required to scroll a selection into view for search/replace)
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (Action)delegate
                    {
                        if (ActiveDocumentChanged != null)
                        {
                            ActiveDocumentChanged(this, new DocumentChangedEventArgs(_ActiveDocument));

////                            if (value != null && _ShutDownInProgress == false)
////                            {
////                                if (value.IsFilePathReal)
////                                    _SettingsManager.SessionData.LastActiveFile = value.FilePath;
////                            }
                        }
                    });
                }
            }
        }
        #endregion properties

        #region methods
        public bool CloseDocument(IDocumentBaseViewModel closeMe)
        {
            if (closeMe == null || _Documents.Count <= 0)
                return false;

            if (closeMe == ActiveDocument)
                ActiveDocument = null;

            _Documents.Remove(closeMe);

            var disposMe = closeMe as IDisposable;
            if (disposMe != null)
                disposMe.Dispose();

            return true;
        }

        /// <summary>
        /// Invoke this method before application shut down
        /// to save all relevant user settings for recovery on appliaction re-start.
        /// </summary>
        public void SaveSettings()
        {
            foreach (var doc in _Documents)
            {
                doc.SaveSettings();
            }
        }

        /// <summary>
        /// Is raised when the user requests to view a file content diff (binary or text).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DocDiffDoc_CompareFilesRequest(object sender, OpenFileDiffEventArgs e)
        {
            var newDoc = new DocDiffDocViewViewModel(this, e.ItemPathA, e.ItemPathB);

            var oldDoc = FindDocument(newDoc.ContentId);

            if (oldDoc == null)              // Add new document and activate it
                AddDocument(newDoc, true);  // CompareFilesCommand is executed via ViewLoadedCommand()
            else
                AcitvateDocument(oldDoc);
        }

        internal void DirectoryCompareShow()
        {
            var newDoc = FindDocument(DirDiffDocViewModel.SetupContentID);

            // Allow only one directory comparer page in setup mode
            // and activate it for the user to bring this to his attention
            if (newDoc == null)
            {
                string leftDir = Properties.Settings.Default.LeftDirPath;
                string rightDir = Properties.Settings.Default.RightDirPath;

                // Or create a new directory setup page if their is currently none to use
                newDoc = new DirDiffDocViewModel(this, leftDir, rightDir);
                AddDocument(newDoc, true);
            }
            else
                AcitvateDocument(newDoc);
        }

        internal IDocumentBaseViewModel FindDocument(string contentId)
        {
            return _Documents.FirstOrDefault(i => i.ContentId == contentId);
        }

        internal void AddDocument(IDocumentBaseViewModel newDoc, bool activateDocument)
        {
            _Documents.Add(newDoc);

            if(activateDocument == true)
                AcitvateDocument(newDoc);
        }

        internal void AcitvateDocument(IDocumentBaseViewModel newDoc)
        {
            ActiveDocument = newDoc;
        }

        #region IDisposable
        /// <summary>
        /// Standard dispose method of the <seealso cref="IDisposable" /> interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Source: http://www.codeproject.com/Articles/15360/Implementing-IDisposable-and-the-Dispose-Pattern-P
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed == false)
            {
                if (disposing == true)
                {
                    var docs = _Documents.ToArray();
                    foreach (var item in docs)
                    {
                        var disposMe = item as IDisposable;
                        if (disposMe != null)
                        {
                            _Documents.Remove(item);
                            disposMe.Dispose();
                        }
                    }
                }

                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.
            }

            _Disposed = true;

            //// If it is available, make the call to the
            //// base class's Dispose(Boolean) method
            ////base.Dispose(disposing);
        }
        #endregion IDisposable
        #endregion methods
    }
}
