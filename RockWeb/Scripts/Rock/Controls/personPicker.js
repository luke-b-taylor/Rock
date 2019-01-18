(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.personPicker = (function () {
        var PersonPicker = function (options) {
            this.controlId = options.controlId;
            this.restUrl = options.restUrl;
            this.restDetailUrl = options.restDetailUrl;
            this.defaultText = options.defaultText || '';
            this.iScroll = null;
            this.$pickerControl = $('#' + this.controlId);
            this.$pickerScrollContainer = this.$pickerControl.find('.js-personpicker-scroll-container');
        };

        PersonPicker.prototype.initializeEventHandlers = function () {
            var controlId = this.controlId,
                restUrl = this.restUrl,
                restDetailUrl = this.restDetailUrl || (Rock.settings.get('baseUrl') + 'api/People/GetSearchDetails'),
                defaultText = this.defaultText;

            var $pickerControl = this.$pickerControl;
            var $searchInput = $pickerControl.find('.js-personpicker-searchinput');
            var $searchResults = $pickerControl.find('.js-personpicker-searchresults');
            var $pickerToggle = $pickerControl.find('.js-personpicker-toggle');
            var $pickerMenu = $pickerControl.find('.js-personpicker-menu');
            var $pickerSelect = $pickerControl.find('.js-personpicker-select');
            var $pickerSelectNone = $pickerControl.find('.js-picker-select-none');
            var $pickerPersonId = $pickerControl.find('.js-person-id');
            var $pickerPersonName = $pickerControl.find('.js-person-name');
            var $pickerCancel = $pickerControl.find('.js-personpicker-cancel');

            var includeBusinesses = $pickerControl.find('.js-include-businesses').val() == '1' ? 'true' : 'false';

            var promise = null;
            var lastSelectedPersonId = null;

            $searchInput.autocomplete({
                source: function (request, response) {

                    // abort any searches that haven't returned yet, so that we don't get a pile of results in random order
                    if (promise && promise.state() === 'pending') {
                        promise.abort();
                    }

                    promise = $.ajax({
                        url: restUrl + "?name=" + encodeURIComponent(request.term) + "&includeHtml=false&includeDetails=false&includeBusinesses=" + includeBusinesses + "&includeDeceased=true",
                        dataType: 'json'
                    });

                    promise.done(function (data) {
                        $searchResults.html('');
                        response($.map(data, function (item) {
                            return item;
                        }));

                        exports.personPickers[controlId].updateScrollbar();
                    });

                    promise.fail(function (xhr, status, error) {
                        console.log(status + ' [' + error + ']: ' + xhr.responseText);
                        var errorCode = xhr.status;
                        if (errorCode == 401) {
                            $searchResults.html("<li class='text-danger'>Sorry, you're not authorized to search.</li>");
                        }
                    });
                },
                minLength: 3,
                html: true,
                appendTo: $searchResults,
                pickerControlId: controlId,
                messages: {
                    noResults: function () { },
                    results: function () { }
                }
            }).data('ui-autocomplete')._renderItem = function ($ul, item) {
                if (this.options.html) {
                    // override jQueryUI autocomplete's _renderItem so that we can do Html for the listitems
                    // derived from http://github.com/scottgonzalez/jquery-ui-extensions

                    var inactiveWarning = "";

                    if (!item.IsActive && item.RecordStatus) {
                        inactiveWarning = " <small>(" + item.RecordStatus + ")</small>";
                    }

                    var quickSummaryInfo = "";
                    if (item.FormattedAge || item.SpouseName) {
                        quickSummaryInfo = " <small class='rollover-item text-muted'>";
                        if (item.FormattedAge) {
                            quickSummaryInfo += "Age: " + item.FormattedAge;
                        }

                        if (item.SpouseName) {
                            if (item.FormattedAge) {
                                quickSummaryInfo += "; ";
                            }

                            quickSummaryInfo += "Spouse: " + item.SpouseName;
                        }

                        quickSummaryInfo += "</small>";
                    }

                    var $div = $('<div/>').attr('class', 'radio'),

                        $label = $('<label/>')
                            .html('<span class="label-text">' + item.Name + inactiveWarning + quickSummaryInfo + '</span><i class="fa fa-refresh fa-spin margin-l-md loading-notification" style="display: none; opacity: .4;"></i>')
                            .prependTo($div),

                        $radio = $('<input type="radio" name="person-id" />')
                            .attr('id', item.Id)
                            .attr('value', item.Id)
                            .prependTo($label),

                        $li = $('<li/>')
                            .addClass('picker-select-item js-picker-select-item')
                            .attr('data-person-id', item.Id)
                            .attr('data-person-name', item.Name)
                            .html($div),

                        $resultSection = $(this.options.appendTo);

                    if (item.PickerItemDetailsHtml) {
                        $(item.PickerItemDetailsHtml).appendTo($li);
                    }
                    else {
                        var $itemDetailsDiv = $('<div/>')
                            .addClass('picker-select-item-details js-picker-select-item-details clearfix')
                            .attr('data-has-details', false)
                            .hide();

                        $itemDetailsDiv.appendTo($li);
                    }

                    if (!item.IsActive) {
                        $li.addClass('is-inactive');
                    }

                    return $resultSection.append($li);
                }
                else {
                    return $('<li></li>')
                        .data('item.autocomplete', item)
                        .append($('<a></a>').text(item.label))
                        .appendTo($ul);
                }
            };

            $pickerToggle.click(function (e) {
                e.preventDefault();
                $(this).toggleClass("active");
                $pickerMenu.toggle(0, function () {
                    exports.personPickers[controlId].updateScrollbar();
                    $(this).find('.picker-search').focus();
                });
            });

            $pickerControl.on('click', '.js-picker-select-item', function (e) {
                if (e.type == 'click' && $(e.target).is(':input') == false) {
                    // only process the click event if it has bubbled up to the input tag
                    return;
                }

                e.stopPropagation();

                var $selectedItem = $(this).closest('.js-picker-select-item');
                var $itemDetails = $selectedItem.find('.js-picker-select-item-details');

                var selectedPersonId = $selectedItem.attr('data-person-id');

                if ($itemDetails.is(':visible')) {

                    if (selectedPersonId == lastSelectedPersonId && e.type == 'click') {
                        // if they are clicking the same person twice in a row (and the details are done expanding), assume that's the one they want to pick
                        $pickerSelect[0].click();
                    } else {

                        // if it is already visible but isn't the same one twice, just leave it open
                    }
                }

                // hide other open details
                $('.js-picker-select-item-details', $pickerControl).filter(':visible').each(function () {
                    var $el = $(this),
                        currentPersonId = $el.closest('.js-picker-select-item').attr('data-person-id');

                    if (currentPersonId != selectedPersonId) {
                        $el.slideUp();
                        exports.personPickers[controlId].updateScrollbar();
                    }
                });

                lastSelectedPersonId = selectedPersonId;

                if ($itemDetails.attr('data-has-details') == 'false') {
                    // add a spinner in case we have to wait on the server for a little bit
                    var $spinner = $selectedItem.find('.loading-notification');
                    $spinner.fadeIn(800);

                    // fetch the search details from the server
                    $.get(restDetailUrl + '?Id=' + selectedPersonId, function (responseText, textStatus, jqXHR) {
                        $itemDetails.attr('data-has-details', true);

                        // hide then set the html so that we can get the slideDown effect
                        $itemDetails.stop().hide().html(responseText);
                        showItemDetails($itemDetails);

                        $spinner.stop().fadeOut(200);
                    });
                } else {
                    showItemDetails($selectedItem.find('.picker-select-item-details:hidden'));
                }
            });

            var showItemDetails = function ($itemDetails) {
                if ($itemDetails.length) {
                    $itemDetails.slideDown(function () {
                        exports.personPickers[controlId].updateScrollbar();
                    });
                }
            }

            $pickerControl.hover(
                function () {

                    // only show the X if there is something picked
                    if ($pickerPersonId.val() || '0' !== '0') {
                        $pickerSelectNone.stop().show();
                    }
                },
                function () {
                    $pickerSelectNone.fadeOut(500);
                });

            $pickerCancel.click(function () {
                $pickerMenu.slideUp(function () {
                    exports.personPickers[controlId].updateScrollbar();
                });
            });

            $pickerSelectNone.click(function (e) {

                var selectedValue = '0',
                    selectedText = defaultText;
                
                $pickerPersonId.val(selectedValue);
                $pickerPersonName.val(selectedText);
            });

            var setSelectedPerson = function (selectedValue, selectedText) {
                var selectedPersonLabel = $pickerControl.find('.js-personpicker-selectedperson-label');

                $pickerPersonId.val(selectedValue);
                $pickerPersonName.val(selectedText);

                selectedPersonLabel.val(selectedValue);
                selectedPersonLabel.text(selectedText);

                $pickerMenu.slideUp();
            }

            $pickerSelect.click(function () {
                var $radInput = $pickerControl.find('input:checked'),
                    selectedValue = $radInput.val(),
                    selectedText = $radInput.closest('.js-picker-select-item').attr('data-person-name');

                setSelectedPerson(selectedValue, selectedText);
            });

            $('.js-select-self', $pickerControl).on('click', function () {
                var selectedValue = $('.js-self-person-id', $pickerControl).val(),
                    selectedText = $('.js-self-person-name', $pickerControl).val();

                setSelectedPerson(selectedValue, selectedText);

                // fire the postBack of the btnSelect if there is one
                var postBackUrl = $pickerSelect.prop('href');
                if (postBackUrl) {
                    window.location = postBackUrl;
                }
            });
        };

        PersonPicker.prototype.updateScrollbar = function () {
            var self = this;

            // first, update this control's scrollbar, then the modal's

            if (self.$pickerScrollContainer.is(':visible')) {
                if (self.iScroll) {
                    self.iScroll.refresh();
                }
            }

            // update the outer modal scrollbar
            Rock.dialogs.updateModalScrollBar(this.controlId);
        }

        PersonPicker.prototype.initialize = function () {

            this.iScroll = new IScroll($('.viewport', this.$pickerControl)[0], {
                mouseWheel: true,
                indicators: {
                    el: $('.track', this.$pickerScrollContainer)[0],
                    interactive: true,
                    resize: false,
                    listenY: true,
                    listenX: false,
                },
                click: false,
                preventDefaultException: { tagName: /.*/ }
            });

            this.initializeEventHandlers();
        };

        var exports = {
            personPickers: {},
            findControl: function (controlId) {
                return exports.personPickers[controlId];
            },
            initialize: function (options) {
                if (!options.controlId) throw '`controlId` is required.';
                if (!options.restUrl) throw '`restUrl` is required.';

                var personPicker = new PersonPicker(options);
                exports.personPickers[options.controlId] = personPicker;
                personPicker.initialize();
            }
        };

        return exports;
    }());
}(jQuery));
