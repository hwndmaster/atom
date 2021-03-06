using System.Diagnostics.CodeAnalysis;

namespace Genius.Atom.UI.Forms;

public interface IUserInteraction
{
    /// <summary>
    ///     Shows a message box to a user with buttons Yes and No.
    /// </summary>
    /// <param name="message">A message to show.</param>
    /// <param name="title">A title of the message box.</param>
    /// <returns>Returns true if user has selected YES. Otherwise returns false.</returns>
    bool AskForConfirmation(string message, string title);

    /// <summary>
    ///     Shows an information popup message to a user.
    /// </summary>
    /// <param name="message">A message content.</param>
    void ShowInformation(string message);

    /// <summary>
    ///     Shows a warning popup message to a user.
    /// </summary>
    /// <param name="message">A message content.</param>
    void ShowWarning(string message);
}

[ExcludeFromCodeCoverage]
internal sealed class UserInteraction : IUserInteraction
{
    public bool AskForConfirmation(string message, string title)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo);
        return result == MessageBoxResult.Yes;
    }

    public void ShowInformation(string message)
    {
        MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowWarning(string message)
    {
        MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
