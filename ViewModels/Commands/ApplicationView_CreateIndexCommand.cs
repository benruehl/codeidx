using CodeIDX.Services.Lucene;
using CodeIDX.ViewModels.Services;
using CodeIDX.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeIDX.ViewModels.Commands
{
    public class ApplicationView_CreateIndexCommand : ViewModelCommand<ApplicationViewModel>
    {

        public static ApplicationView_CreateIndexCommand Instance = new ApplicationView_CreateIndexCommand();

        protected override void Execute(ApplicationViewModel contextViewModel)
        {
            IndexViewModel dialogModel = new IndexViewModel();
            IndexDialog dialog = new IndexDialog
            {
                DataContext = dialogModel,
                Owner = App.Current.MainWindow,
                IsNew = true,
                ResizeMode = System.Windows.ResizeMode.NoResize
            };

            if (dialog.ShowDialog() == true)
            {
                CreateIndex(contextViewModel, dialogModel);
            }
        }

        private async void CreateIndex(ApplicationViewModel contextViewModel, IndexViewModel indexModel)
        {
            CancellationToken cancelToken;
            if (!contextViewModel.BeginOperation(StatusKind.Indexing, out cancelToken))
                return;

            try
            {
                string newIndexDirectory = indexModel.IndexDirectory;
                await Task.Run(() =>
                {
                    LuceneIndexer.Instance.CreateIndexDirectory(indexModel.SourceDirectories, indexModel.FileFilters, newIndexDirectory, cancelToken);
                    if (cancelToken.IsCancellationRequested)
                        return;

                    indexModel.LastFullRefresh = DateTime.Now;
                    indexModel.SaveIndexFile();
                },
                cancelToken);

                if (cancelToken.IsCancellationRequested)
                {
                    if (Directory.Exists(newIndexDirectory))
                        Directory.Delete(newIndexDirectory, true);

                    return;
                }

                contextViewModel.EndOperation();
                await ApplicationViewService.LoadIndex(indexModel.IndexFile);
            }
            finally
            {
                contextViewModel.EndOperation();
            }
        }
    }
}
