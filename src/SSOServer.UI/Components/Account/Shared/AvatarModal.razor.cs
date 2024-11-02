using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace SSOServer.UI.Components.Account.Shared;

public partial class AvatarModal : ComponentBase
{
    public string? AvatarImageUrl => avatarImageUrl;

    private bool showModal = false;
    private string? avatarImageUrl;
    private string? uploadedImageUrl;

    // Открытие модального окна
    public void OpenModal()
    {
        showModal = true;
    }

    // Закрытие модального окна
    public void CloseModal()
    {
        showModal = false;
    }

    // Загрузка и отображение выбранного изображения
    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null)
        {
            using var stream = file.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var imageBytes = ms.ToArray();
            uploadedImageUrl = $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}";
        }
    }

    // Подтверждение и сохранение нового аватара
    private void ConfirmChange()
    {
        avatarImageUrl = uploadedImageUrl; // Сохраняем изображение как новый аватар
        CloseModal();
    }
}