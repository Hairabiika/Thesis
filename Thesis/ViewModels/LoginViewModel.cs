using System.ComponentModel.DataAnnotations;

namespace Thesis.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Моля, въведете имейл адрес")]
        [EmailAddress(ErrorMessage = "Моля, въведете реален имейл адрес")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Моля, въведете парола")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
