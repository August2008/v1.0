﻿
Hero.saveAction = undefined;
Hero.editAction = undefined;

function Hero() {
    // server model
    this.HeroId = null;
    this.MilitaryRankId = null;
    this.MilitaryGroupId = null;
    this.FirstName = null;
    this.LastName = null;
    this.MiddleName = null;
    this.Dob = null;
    this.Died = null;
    this.Biography = null;
    // client model
    this.photos = new Array();
};
Hero.init = function() {
    // editor dialog
    $('#hero-dialog').dialog({
        autoOpen: false,
        modal: true,
        width: 710,
        title: 'Add Hero',
        buttons: {
            Save: function () {
                $.hero.save();
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });    
    // dialog open
    $('#createButton').click(function() {
        $.hero.edit();
    });
};
Hero.prototype.edit = function() {
    $.ajax({
        url: Hero.editAction,
        method: 'GET',
        success: function(result) {
            $('#hero-dialog').html(result);
            $('#hero-dialog').dialog('open');
        },
        error: function(xhr, ajaxOptions, thrownError) {
            alert(xhr.status);
            alert(thrownError);
        }
    });
};
Hero.prototype.toFormData = function () {
    //debugger;
    var formData = new FormData();
    for (var prop in this) {
        var elmnt = $('#' + prop);
        if (this.hasOwnProperty(prop) && elmnt.length != 0) {
            formData.append(prop, (this[prop] = elmnt.val()) || '');
        }
    }
    for (var i = 0; i < this.photos.length; i++) {
        formData.append("images", this.photos[i]);
    }
    return formData;
};
Hero.prototype.onFileChange = function (sender, args) {
    var i = 0;
    var len = sender.files.length;
    var img;
    var reader;
    var file;

    for (; i < len; i++) {
        file = sender.files[i];
        if (!!file.type.match(/image.*/)) {
            if (window.FileReader) {
                reader = new FileReader();
                reader.onloadend = function(e) {
                    $.hero.addPhoto(e.target.result, file);
                };
                reader.readAsDataURL(file);
            }
        }        
    }
};
Hero.prototype.addPhoto = function (src, file) {
    //debugger;    
    var img = document.createElement('img');
    img.src = src;

    var maxWidth = 125;
    var maxHeight = 125;
    var adjust = 0;
    var percent = 0;
    var wPercent = 0;
    var hPercent = 0;
    var wAdjust = 0;
    var hAdjust = 0;

    var container = $('#upload-images');
    var div = $('<div class="thumb-img-box"></div>');

    // 1. append first
    container.append(div.append(img));

    // 2. get dimmensions
    var imageWidth = img.width;
    var imageHeight = img.height;

    // 3. calculate
    if (imageWidth >= (maxWidth - wAdjust)) {
        wPercent = (imageWidth - (maxWidth - adjust)) / imageWidth;
    }
    if (imageHeight >= (maxHeight - hAdjust)) {
        hPercent = (imageHeight - (maxHeight - adjust)) / imageHeight;
    }
    percent = Math.max(wPercent, hPercent);
    if (percent != 0) {
        imageHeight = Math.round(imageHeight - (imageHeight * percent));
        imageWidth = Math.round(imageWidth - (imageWidth * percent));
    }
    // 3. and scale to size
    img.width = imageWidth;
    img.height = imageHeight;

    this.photos[this.photos.length] = file;
};
Hero.prototype.save = function () {
    //debugger;
    var formData = $.hero.toFormData();
    $.ajax({
        url: Hero.saveAction,
        type: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function(res) {
            alert(res);
        },
        error: function(jqXhr, textStatus, errorMessage) {
            alert(errorMessage);
        }
    });
};

$.hero = new Hero();

/*
*/