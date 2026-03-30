/**
 * Client-side Validation Configuration
 * Uses jQuery Validation for form validation with custom error messages
 */

$(document).ready(function() {
    // Global jQuery Validation Settings
    $.validator.setDefaults({
        errorClass: 'is-invalid',
        validClass: 'is-valid',
        errorPlacement: function(error, element) {
            error.insertAfter(element).addClass('d-block mt-2 text-danger small fw-bold');
        },
        highlight: function(element) {
            $(element).addClass('is-invalid').removeClass('is-valid');
            $(element).closest('.form-group').find('.invalid-feedback').show();
        },
        unhighlight: function(element) {
            $(element).removeClass('is-invalid').addClass('is-valid');
            $(element).closest('.form-group').find('.invalid-feedback').hide();
        }
    });

    // =============================================
    // REGISTER FORM VALIDATION
    // =============================================
    if ($('#registerForm').length) {
        $('#registerForm').validate({
            rules: {
                FirstName: {
                    required: true,
                    minlength: 2,
                    maxlength: 50
                },
                LastName: {
                    required: true,
                    minlength: 2,
                    maxlength: 50
                },
                Email: {
                    required: true,
                    email: true
                },
                PhoneNumber: {
                    required: true,
                    minlength: 10,
                    maxlength: 15
                },
                Password: {
                    required: true,
                    minlength: 6,
                    strongPassword: true
                },
                ConfirmPassword: {
                    required: true,
                    equalTo: '#Password'
                },
                Role: {
                    required: true
                }
            },
            messages: {
                FirstName: {
                    required: 'First name is required',
                    minlength: 'First name must be at least 2 characters',
                    maxlength: 'First name cannot exceed 50 characters'
                },
                LastName: {
                    required: 'Last name is required',
                    minlength: 'Last name must be at least 2 characters',
                    maxlength: 'Last name cannot exceed 50 characters'
                },
                Email: {
                    required: 'Email is required',
                    email: 'Please enter a valid email address'
                },
                PhoneNumber: {
                    required: 'Phone number is required',
                    minlength: 'Phone number must be at least 10 digits',
                    maxlength: 'Phone number cannot exceed 15 digits'
                },
                Password: {
                    required: 'Password is required',
                    minlength: 'Password must be at least 6 characters',
                    strongPassword: 'Password must contain at least one uppercase letter, one lowercase letter, and one digit'
                },
                ConfirmPassword: {
                    required: 'Confirm password is required',
                    equalTo: 'Passwords do not match'
                },
                Role: {
                    required: 'Please select a role'
                }
            },
            submitHandler: function(form) {
                // Show loading indicator
                $(form).find('button[type="submit"]').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Registering...');
                form.submit();
            }
        });
    }

    // =============================================
    // LOGIN FORM VALIDATION
    // =============================================
    if ($('#loginForm').length) {
        $('#loginForm').validate({
            rules: {
                Email: {
                    required: true,
                    email: true
                },
                Password: {
                    required: true,
                    minlength: 6
                }
            },
            messages: {
                Email: {
                    required: 'Email is required',
                    email: 'Please enter a valid email address'
                },
                Password: {
                    required: 'Password is required',
                    minlength: 'Password must be at least 6 characters'
                }
            },
            submitHandler: function(form) {
                // Show loading indicator
                $(form).find('button[type="submit"]').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Logging in...');
                form.submit();
            }
        });
    }

    // =============================================
    // PROPERTY CREATE/EDIT FORM VALIDATION
    // =============================================
    if ($('#propertyForm').length) {
        $('#propertyForm').validate({
            rules: {
                Title: {
                    required: true,
                    minlength: 5,
                    maxlength: 200
                },
                Description: {
                    required: true,
                    minlength: 20,
                    maxlength: 5000
                },
                Price: {
                    required: true,
                    number: true,
                    min: 0.01
                },
                Location: {
                    required: true,
                    minlength: 5
                },
                Area: {
                    required: true,
                    number: true,
                    min: 1
                },
                Bedrooms: {
                    required: true,
                    number: true,
                    min: 0
                },
                Bathrooms: {
                    required: true,
                    number: true,
                    min: 0
                },
                PropertyType: {
                    required: true
                }
            },
            messages: {
                Title: {
                    required: 'Property title is required',
                    minlength: 'Title must be at least 5 characters',
                    maxlength: 'Title cannot exceed 200 characters'
                },
                Description: {
                    required: 'Description is required',
                    minlength: 'Description must be at least 20 characters',
                    maxlength: 'Description cannot exceed 5000 characters'
                },
                Price: {
                    required: 'Price is required',
                    number: 'Please enter a valid price',
                    min: 'Price must be greater than 0'
                },
                Location: {
                    required: 'Location is required',
                    minlength: 'Location must be at least 5 characters'
                },
                Area: {
                    required: 'Area is required',
                    number: 'Please enter a valid area',
                    min: 'Area must be at least 1'
                },
                Bedrooms: {
                    required: 'Number of bedrooms is required',
                    number: 'Please enter a valid number',
                    min: 'Bedrooms cannot be negative'
                },
                Bathrooms: {
                    required: 'Number of bathrooms is required',
                    number: 'Please enter a valid number',
                    min: 'Bathrooms cannot be negative'
                },
                PropertyType: {
                    required: 'Please select a property type'
                }
            }
        });
    }

    // =============================================
    // CUSTOM VALIDATORS
    // =============================================

    // Strong Password Validator
    $.validator.addMethod('strongPassword', function(value, element) {
        return this.optional(element) || 
               /(?=.*[a-z])/.test(value) &&  // At least one lowercase
               /(?=.*[A-Z])/.test(value) &&  // At least one uppercase
               /(?=.*\d)/.test(value);        // At least one digit
    }, 'Password must contain at least one uppercase letter, one lowercase letter, and one digit');

    // Phone Number Validator (for Egypt numbers)
    $.validator.addMethod('egyptPhone', function(value, element) {
        return this.optional(element) || /^(\+20|0)?1[0125]\d{8}$/.test(value);
    }, 'Please enter a valid Egyptian phone number');
});

/**
 * Helper function to disable form submission button on validation
 */
function disableSubmitButton(formId) {
    const form = document.getElementById(formId);
    if (form) {
        const submitBtn = form.querySelector('button[type="submit"]');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Processing...';
        }
    }
}

/**
 * Helper function to re-enable form submission button
 */
function enableSubmitButton(formId) {
    const form = document.getElementById(formId);
    if (form) {
        const submitBtn = form.querySelector('button[type="submit"]');
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = 'Submit';
        }
    }
}
