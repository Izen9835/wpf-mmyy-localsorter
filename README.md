# WPF Folder Sorter
Application made for folder sorting on company databases   
All sensitive code has been obscured.  
In this readme, I detail my findings from developing this project.    

![image](https://github.com/user-attachments/assets/29fb3212-cd29-48ba-a848-79292cf3cd49)


## 1. Architecture (MVC Model)
```
FolderMMYYSorter_2/
└── MVVM/
    ├── View/
    │   ├── MainWindow.xaml
    │   ├── P1_srcdir.xaml
    │   ├── P2_options.xaml
    │   ├── P3a_SQLOption.xaml
    │   ├── P3_destdir.xaml
    │   ├── P4_summary.xaml
    │   └── P5_complete.xaml
    └── ViewModel/
        ├── MainViewModel.cs
        ├── P1_srcdir_VM.cs
        ├── P2_options_VM.cs
        ├── P3a_SQLOption_VM.cs
        ├── P3_destdir_VM.cs
        ├── P4_summary_VM.cs
        ├── P5_complete_VM.cs
        └── _baseviewmodel.cs
```
### Dynamic Content Area
```xaml
<!-- MainWindow.xaml -->

<!--Top Row: Title and Instruction Card-->
<Border Grid.Row="0" Background="NavajoWhite" Padding="16"> ... </Border>

<!--Middle Row: Dynamical Content Area (Both title and function are rendered here)-->
<ContentControl Grid.Row="1" Margin="20" Content="{Binding CurrentPage}" Background="Bisque"/>

<!--Bottom Row: Navigation buttons-->
<StackPanel Grid.Row="2"> ... </StackPanel>
```
Using User Control (WPF) as the View, and setting up the corresponding VM, we can essentially switch between pages, with each page having a different function.
```cs
// MainViewModel.cs

_pages = new List<_baseviewmodel>
{
    new P1_srcdir_VM(_FileExplorer),
    new P2_options_VM(_FileExplorer),
    new P3_destdir_VM(_FileExplorer),
    new P3a_SQLOption_VM(_FileExplorer),
    new P4_summary_VM(_FileExplorer),
    new P5_complete_VM(_FileExplorer),
};
_currentIndex = 0;
CurrentPage = _pages[_currentIndex];
```
As seen above, ```CurrentPage``` is binded to the dynamic content area.  
To change pages we simply set ```CurrentPage``` to an instance to one of the User Control view models.  
To bind the individual view models to the view, we add a DataTemplate to ```App.xaml```
```xaml
<!-- App.xaml -->

<Application xmlns:views="clr-namespace:FolderMMYYSorter_2.MVVM.View"
             xmlns:viewmodel="clr-namespace:FolderMMYYSorter_2.MVVM.ViewModel"
              ... >
    <Application.Resources>
        <DataTemplate DataType="{x:Type viewmodel:P1_srcdir_VM}">
            <views:P1_srcdir/>
        </DataTemplate>
        <DataTemplate DataType="{x:Type viewmodel:P2_options_VM}">
            <views:P2_options/>
        </DataTemplate>
        ...
    </Application.Resources>
</Application>
```
Each view-viewmodel is then binded and compartmentalised into separate files.  
  
### Compartmentalisation
```cs
  // P3_destdir_VM.cs

  class P3_destdir_VM : _baseviewmodel
  {
      public RelayCommand BrowseCommand { get; set; }

      public P3_destdir_VM(FileExplorer fileExplorer) : base(fileExplorer)
      {
          Title = "Select Destination";
          Instructions = "Select a destination directory. (where will the output be stored)";

          BrowseCommand = new RelayCommand(o => _FileExplorer.destFileDialog());
      }
  }
```
All view models are children of ```_baseviewMmdel.cs```, which is useful for setting default values for e.g. for Title and Instruction page and then changing them dynamically.  
Also, it allows us to share the same instance of FileExplorer across the multiple pages, which will be important for later.
```cs
// _baseviewmodel.cs

class _baseviewmodel : INotifyPropertyChanged
{
    public FileExplorer _FileExplorer { get; set; }

    // event handler to reflect changes in ViewModel to UI
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName) { ... }

    public string Title { get; set; }
    public string Instructions { get; set; }
    public string nextButtonText { get; set; }

    public _baseviewmodel(FileExplorer fileExplorer)
    {
        // use same instance of file explorer for every sub page
        _FileExplorer = fileExplorer;

        // default title and instructions
        Title = "Default Title Text";
        Instructions = "lorem ipsum default instructions text";
        nextButtonText = "Next";
    }
}
```

## 2. ```FileExplorer.cs```
"this is where the magic happens"  

### Async
Pretty much all the main functions here must be async as they are IO  
On top of ```Task.Run()```, using ```Parallel.ForEach``` drastically improved the performance of the IO actions.  
For tracking of errors during each thread, a simple ```gotError``` flag was sufficient (or the idea of it)
```cs
// FileExplorer.cs

public async Task UpdateDisplayedFilesAsync() {
            await Task.Run(() =>
            {
                // 1. Process top-level files/folders
                  ...

                // 2. Process subdirectories
                  ...
                Parallel.ForEach(subDirs, subDir =>[](url)
                {
                    try { ... }
                    catch (UnauthorizedAccessException)
                    {
                        gotError = true;
                        ...
                    }
                });
            });

            if (gotError) { ... }
        }
```
Adding on to what was learnt in [WPF TCP/IP Chat Application, CancellationTokenSource](https://github.com/Izen9835/NETWPF-tcpip-chat?tab=readme-ov-file#4-cancellationtokensource), to halt threads under ```Parallel.ForEach``` you must enter the CTS as a parameter as well, as such:
```cs
// FileExplorer.cs

public async Task<bool> execute(CancellationTokenSource _cts, ...)
{
    // Process in parallel
    await Task.Run(() =>
    {
        try
        {
            Parallel.ForEach(src,
                             new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount,
                                                   CancellationToken = _cts.Token },
                             file => { ... });
        }
        catch (OperationCanceledException ex)
        {
            MessageBox.Show(
                "Operation cancelled.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }

    }, _cts.Token); // so can cancel via cts

    ...
}
```
In this case, the CancellationTokenSource was passed in from MainViewModel, where the UI's cancel button is binded.  

## Progress Bar
![image](https://github.com/user-attachments/assets/184ee9b0-b52c-452c-973c-3b646c134754)  

Using an IProgress to show user updates on the progress of async actions  
```cs
// FileExplorer.cs

        public async Task<bool> execute(...  IProgress<int> ProgressValue = null, IProgress<string> CurrentItem = null)
        {
            // for progress bar
            int totalItems = src.Count;
            int processedItems = 0;

            await Task.Run(() =>
            {
                    Parallel.ForEach(... , file =>
                    {
                        try
                        {

                            ...

                            // update UI periodically (not with every object)
                            // otherwise UI will simply seize
                            int reportFrequency = 10;
                            int newCount = Interlocked.Increment(ref processedItems);
                            if (newCount % reportFrequency == 0 || newCount == totalItems)
                            {
                                ProgressValue?.Report((newCount * 100) / totalItems);
                                CurrentItem?.Report(file.name);
                            }

                        }
                        catch (Exception ex) { ... }
                    });

            });
            ...
        }
```
```cs
// P4_Summary_VM.cs

public async Task<bool> Execute_w_Prog_Bar(CancellationTokenSource _cts)
{
    ...

    var progress = new Progress<int>(percent =>
    {
        Win32.Application.Current.Dispatcher.Invoke(() =>
        {
            ProgressValue = percent;
            ProgressText = $"{percent}%";
        });
    });

    var currentItemProgress = new Progress<string>(item =>
    {
        Win32.Application.Current.Dispatcher.Invoke(() =>
        {
            CurrentItemText = item;
        });
    });

    bool success = await _FileExplorer.execute(_cts, progress, currentItemProgress);
    ...
}
```
Essentially, when we call ```CurrentItem?.Report(file.name)```, a background thread will call the UI thread to update the disp  
You might notice that we are passing an ```Program``` type object as a parameter that expects ```IProgram```  
This is because ```Program``` is a class that inherits from the ```IProgram``` interface. [Read more here.](https://stackoverflow.com/a/68663258)


### Checking for issues before execution
Small trick that allows a popup box to show a list of problems for the user to resolve before execution is allowed.
```cs
// FileExplorer.cs

List<string> errors = [];

if (src == null || src.Count == 0) errors.Add("No items found in source directory");
if (!Directory.Exists(DestDirectory)) errors.Add("Destination is invalid");
if (FolderName == "") errors.Add("Fill in a folder name");
if (isUsingSQLDB && !_SqlHelper.hasSQLData())
{
    if (_SqlHelper.isGettingFileTypes) errors.Add("SQL data still loading, please wait");
    else errors.Add("no SQL data found");
}
    
if (errors.Count > 0)
{
    MessageBox.Show(
        $"Missing requirements:\n\n• {string.Join("\n• ", errors)}",
        "Action Cannot Proceed",
        MessageBoxButtons.OK,
        MessageBoxIcon.Exclamation);
    ...
}
```
