// app-helpers.js
// Generic AJAX form submitter with SweetAlert2 toast notifications

window.AppHelpers = window.AppHelpers || {};

(function (helpers) {

    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3500,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });

    function showToast(isSuccess, message) {
        Toast.fire({
            icon: isSuccess ? 'success' : 'error',
            title: message || (isSuccess ? 'Success!' : 'An error occurred.')
        });
    }

    /**
     * Binds a form to submit via AJAX and show SweetAlert2 notifications.
     * @param {string} formSelector - The jQuery selector for the form (e.g., '#myForm', '.ajax-form').
     * @param {function} [onSuccess] - Optional callback function to run on success. Receives the response data.
     */
    helpers.bindAjaxForm = function (formSelector, onSuccess) {
        $(document).on('submit', formSelector, function (e) {
            e.preventDefault();

            const form = $(this);
            const url = form.attr('action');
            const method = form.attr('method') || 'POST';
            const data = form.serialize();
            const submitButton = form.find('button[type="submit"]');
            const originalButtonText = submitButton.html();

            // Disable button and show loading state
            submitButton.prop('disabled', true).html('<i class="spinner-border spinner-border-sm me-2"></i>Processing...');

            $.ajax({
                url: url,
                type: method,
                data: data,
                success: function (response) {
                    if (response.success) {
                        showToast(true, response.message);

                        if (onSuccess && typeof onSuccess === 'function') {
                            onSuccess(response);
                        } else if (response.redirectUrl) {
                            setTimeout(() => {
                                window.location.href = response.redirectUrl;
                            }, 1500); // Wait a bit for the user to see the toast
                        }
                    } else {
                        showToast(false, response.message);
                    }
                },
                error: function (xhr) {
                    let errorMessage = 'An unexpected server error occurred.';
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    }
                    showToast(false, errorMessage);
                },
                complete: function () {
                    // Re-enable button and restore original text
                    submitButton.prop('disabled', false).html(originalButtonText);
                }
            });
        });
    };

})(window.AppHelpers);
