
$(document).ready(function () {
    $("#search_images").on("keyup", function () {
        fetch("/Admin/ImageSlideShow/GetAll", {
            method: "GET"
        }).then(function (response) {
            if (!response.ok) {
                return [];
            } 
            return response.json();
        }).then(function (data) {
            let value = $(this).val().toLowerCase();
            $("#image_slideshows").empty();
            for (var i = 0; i < data.length; i++)
            {
            }
            console.log("test");
        }).catch(function (error) {
            console.log(error);
        })
    });
});