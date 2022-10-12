using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace JiraStatistics.GuiApp.Views;

public class MainWindow : Window
{
    private readonly TextBox _logsTextBox;

    public MainWindow()
    {
        this.InitializeComponent();

        this._logsTextBox = this.FindControl<TextBox>("LogsTextBox");
        this._logsTextBox.PropertyChanged += this.TextBoxOnPropertyChanged;
    }

    private void TextBoxOnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (TextBox.TextProperty.Equals(e.Property))
        {
            this._logsTextBox.CaretIndex = int.MaxValue;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}