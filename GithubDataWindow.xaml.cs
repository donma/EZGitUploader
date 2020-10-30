using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EZGitUploader
{
    /// <summary>
    /// GithubDataWindow.xaml 的互動邏輯
    /// </summary>
    public partial class GithubDataWindow : Window
    {
        public GithubDataWindow()
        {
            InitializeComponent();
        }

        public GithubPersonalInfo GitInfo { get; set; }

        public GitHubClient GitHubClient { get; set; }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private  void Delete(string[] localPath)
        {

            try
            {
                if (localPath == null) return;

                var targets = localPath.Select(x => System.IO.Path.GetFileName(x)).ToArray();
                if (targets == null)
                {
                    return;
                }

                var existingFiles = GitHubClient.Repository.Content.GetAllContentsByRef(GitInfo.UserName, GitInfo.RepoName, GitInfo.Dir+"/", "master").Result;

                if (existingFiles != null)
                {
                    foreach (var f in existingFiles)
                    {
                        if (targets.Any(x => x == f.Name))
                        {
                            //    MessageBox.Show(f.Name);
                            txtResult.Content = "Delete " + f.Name;
                             GitHubClient.Repository.Content.DeleteFile(GitInfo.RepoId, GitInfo.Dir + "/" + f.Name, new DeleteFileRequest("delete file", f.Sha)).RunSynchronously();

                        }
                    }
                }
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message);
            }
        }

        private string UploadFile(string localPath)
        {

            try
            {

                //最後一個參數是否要轉成 base64
                var updateRequest = new UpdateFileRequest("Upload by EZ Github Uploader.", Convert.ToBase64String(File.ReadAllBytes(localPath)), "SHA", false);

                var res = GitHubClient.Repository.Content.UpdateFile(GitInfo.RepoId, GitInfo.Dir + "/" + System.IO.Path.GetFileName(localPath), updateRequest).Result;

                return res.Content.DownloadUrl;
            }
            catch (Exception ex)
            {
                return "";

            }
        }

        private void UpdateListBox()
        {

            listFiles.Items.Clear();

            try
            {
                var existingFiles = GitHubClient.Repository.Content.GetAllContentsByRef(GitInfo.UserName, GitInfo.RepoName, GitInfo.Dir + "/", "master").Result;

                if (existingFiles != null)
                {
                    foreach (var f in existingFiles)
                    {
                        var t = new ListBoxItem();
                        t.Tag = f.DownloadUrl;
                        t.Content = f.Name;
                        listFiles.Items.Add(t);

                    }
                }

            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    txtResult.Content = "Upload Error: " + ex.Message;
                }));

            }
        }

        private void imgDrop_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null)
                {
                    files = files.Where(x => File.Exists(x)).ToArray();
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        txtResult.Content = "No Existed File To Upload.";
                    }));

                }

                if (files != null)
                {


                    Delete(files);

                    var i = 0;
                    foreach (var f in files)
                    {
                        txtResult.Content = "Handling " + f;

                        var res = UploadFile(f);
                    }
                }

                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    txtResult.Content = "Success Upload";
                    UpdateListBox();
                }));


              

            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    txtResult.Content = "No File To Upload";
                }));
            }
        }




        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

            GitHubClient = new GitHubClient(new ProductHeaderValue("DONMATEST"));

            GitHubClient.Credentials = new Credentials(GitInfo.Token);

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
            
                UpdateListBox();
            }));

        }

        private void listFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var target = listFiles.SelectedItem as ListBoxItem;
            txtOutput.Text = txtOutputTemplate.Text.Replace("[CODE]", target.Tag.ToString());
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            UpdateListBox();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                UpdateListBox();
            }));
        }
    }
}
