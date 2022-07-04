
$(document).ready(function () {
    $("#contact_form").submit(async function (event) {
        // Prevent form to submit
        event.preventDefault();

        // Get all the inputs into an array
        var htmlForm = $('#contact_form')[0];
        // Form data to be sent to server.
        const formData = new FormData(htmlForm);
        
        // Anti forgery token
        var token = $("#contact_form input[name=__RequestVerificationToken]").val();
        
        await fetch("/EmailSender/SendEmail", {
            method: "POST", body: formData, headers: {
                "X-ANTI-FORGERY-TOKEN": token,
            }
        }).then(function (response) {
            if (response.ok) {
                return response.json();
            }
            else {
                return null;
            }
        }).then(function (data) {
            if (data == null) {
                console.error("Internal server error!");
            }
            if (data.length == 0) {
                $('#contact_msg').css('display', 'block');
                clearErrorMessages();
                clearInputMessages();
                setTimeout(function () {
                    $('#contact_msg').fadeOut('ease');
                }, 5000); // <-- time in milliseconds
            }
            else {
                // Display error messages from ModelState.
                let errors = 0;
                for (var i = 0; i < data.length; i++) {
                    if (data[i].errorMessage.indexOf("Name") >= 0) {
                        $("#name_error_message_contact").html(data[i].errorMessage);
                        errors += 1;
                    }
                    if (data[i].errorMessage.indexOf("Email") >= 0) {
                        $("#email_error_message_contact").html(data[i].errorMessage);
                        errors += 1;
                    }
                    if (data[i].errorMessage.indexOf("Phone") >= 0) {
                        $("#phone_error_message_contact").html(data[i].errorMessage);
                        errors += 1;
                    }
                    if (data[i].errorMessage.indexOf("Subject") >= 0) {
                        $("#subject_error_message_contact").html(data[i].errorMessage);
                        errors += 1;
                    }
                    if (data[i].errorMessage.indexOf("Message") >= 0) {
                        $("#message_error_message_contact").html(data[i].errorMessage);
                        errors += 1;
                    }
                }
                if (errors != data.length) {
                    console.error("Internal server error! Unkown error message.");
                }
            }
            
            
        }).catch(function (err) {
            console.warn('Something went wrong.', err)
        });
    });
});

function clearErrorMessages() {
    $("#name_error_message_contact").html("");
    $("#email_error_message_contact").html("");
    $("#phone_error_message_contact").html("");
    $("#subject_error_message_contact").html("");
    $("#message_error_message_contact").html("");
}

function clearInputMessages() {
    $("#name_input_contact").val("");
    $("#email_input_contact").val("");
    $("#phone_input_contact").val("");
    $("#subject_input_contact").val("");
    $("#message_input_contact").val("");
}