namespace SimpleAuth.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    /// Defines the update resource owner credentials view model.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UpdateResourceOwnerCredentialsViewModel
    {
        /// <summary>
        /// Gets or sets the login.
        /// </summary>
        /// <value>
        /// The login.
        /// </value>
        public string? Login { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [Required] public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the repeat password.
        /// </summary>
        /// <value>
        /// The repeat password.
        /// </value>
        [Required] public string? RepeatPassword { get; set; }

        /// <summary>
        /// Validates the specified model state.
        /// </summary>
        /// <param name="modelState">State of the model.</param>
        /// <exception cref="ArgumentNullException">modelState</exception>
        public void Validate(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            if (Password != RepeatPassword)
            {
                modelState.AddModelError("NotSamePassword", "The password must be the same");
            }
        }
    }
}
