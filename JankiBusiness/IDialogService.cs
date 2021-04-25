using System.IO;
using System.Threading.Tasks;

namespace JankiBusiness
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationDialog(string title, string content, string positiveOption, string negativeOption);
        Task<string> ShowTextPromptDialog(string title, string initialContent, bool canCancel);
        Task<Stream> OpenFile(params string[] filters);
    }
}