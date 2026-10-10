
document.addEventListener("DOMContentLoaded", function () {

    /*
     * AJACAS Global Form Submit Lock
     *
     * Submit buttons remain disabled until the form
     * passes its client-side validation requirements.
     */

    const forms = document.querySelectorAll("form");

    forms.forEach(function (form) {

        // Allow a form to explicitly opt out
        if (form.dataset.submitLock === "false") {
            return;
        }

        const submitButtons = form.querySelectorAll(
            'button[type="submit"], input[type="submit"]'
        );

        // Nothing to control
        if (submitButtons.length === 0) {
            return;
        }

        /*
         * Only automatically apply the feature to forms that
         * actually contain validation rules.
         *
         * This prevents Delete/Logout/simple action forms
         * from being unnecessarily disabled.
         */
        const hasValidationRules =
            form.querySelector('[data-val="true"]') ||
            form.querySelector('[required]') ||
            form.querySelector('[data-submit-lock="true"]');

        if (!hasValidationRules) {
            return;
        }

        function getFormValidity() {

            /*
             * Prefer ASP.NET Core / jQuery Unobtrusive validation
             * when it is available.
             */
            if (
                window.jQuery &&
                typeof window.jQuery.fn.valid === "function"
            ) {
                const $form = window.jQuery(form);

                // Make sure unobtrusive validation has parsed the form
                if (
                    window.jQuery.validator &&
                    window.jQuery.validator.unobtrusive
                ) {
                    const validator = $form.data("validator");

                    if (!validator) {
                        window.jQuery.validator.unobtrusive.parse(form);
                    }
                }

                const validator = $form.data("validator");

                if (validator) {
                    return validator.checkForm();
                }

                return $form.valid();
            }

            /*
             * Fallback to native browser validation.
             */
            return form.checkValidity();
        }


        function updateSubmitButtons() {

            const isValid = getFormValidity();

            submitButtons.forEach(function (button) {

                button.disabled = !isValid;

                if (isValid) {

                    button.classList.remove("ajacas-submit-disabled");

                    button.removeAttribute("aria-disabled");

                } else {

                    button.classList.add("ajacas-submit-disabled");

                    button.setAttribute("aria-disabled", "true");

                }

            });
        }


        /*
         * Initial state
         */
        updateSubmitButtons();


        /*
         * Re-check whenever the user changes a field.
         */
        form.addEventListener("input", function () {
            updateSubmitButtons();
        });


        form.addEventListener("change", function () {
            updateSubmitButtons();
        });


        form.addEventListener("blur", function () {
            updateSubmitButtons();
        }, true);


        /*
         * Prevent submission if the form somehow tries to submit
         * while still invalid.
         */
        form.addEventListener("submit", function (event) {

            if (!getFormValidity()) {

                event.preventDefault();

                updateSubmitButtons();

            }

        });

    });

});
