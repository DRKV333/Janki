using System.Threading.Tasks;

namespace JankiBusiness
{
    public interface IConfirmationDialogService
    {
        Task<bool> ShowDialog(string title, string content, string positiveOption, string negativeOption);
    }
}