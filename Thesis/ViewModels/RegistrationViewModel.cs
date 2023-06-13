using System.ComponentModel.DataAnnotations;

namespace Thesis.ViewModels
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Моля, въведете имейл адрес")]
        [EmailAddress(ErrorMessage = "Моля, въведете валиден имейл адрес")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Моля, въведете парола")]
        [MinLength(6, ErrorMessage = "Паролата трябва да бъде поне 6 символа")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Моля, потвърдете парола")]
        [Compare(nameof(Password), ErrorMessage = "Паролите не съвпадат")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Моля, въведете своето име")]
        public string Name { get; set; }

        public string Type { get; set; } = "user";
        public bool IsVerified { get; set; } = false;

    }
}
