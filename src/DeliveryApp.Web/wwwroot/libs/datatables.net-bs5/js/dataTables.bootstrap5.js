/*! DataTables Bootstrap 5 Integration
 * ©2011-2020 SpryMedia Ltd - datatables.net/license
 */

(function( factory ){
    if ( typeof define === 'function' && define.amd ) {
        // AMD
        define( ['jquery', 'datatables.net'], function ( $ ) {
            return factory( $, window, document );
        } );
    }
    else if ( typeof exports === 'object' ) {
        // CommonJS
        module.exports = function (root, $) {
            if ( ! root ) root = window;
            if ( ! $ || ! $.fn.dataTable ) $ = require('datatables.net')(root, $).$;
            return factory( $, root, root.document );
        };
    }
    else {
        // Browser
        factory( jQuery, window, document );
    }
}(function( $, window, document, undefined ) {
'use strict';
var DataTable = $.fn.dataTable;

/* Set the defaults for DataTables initialisation */
$.extend( true, DataTable.defaults, {
    dom:
        "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
        "<'row'<'col-sm-12'tr>>" +
        "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
    renderer: 'bootstrap'
} );

/* Default class modification */
$.extend( DataTable.ext.classes, {
    sWrapper:      "dataTables_wrapper dt-bootstrap5",
    sFilterInput:  "form-control form-control-sm",
    sLengthSelect: "form-select form-select-sm",
    sProcessing:   "dataTables_processing card",
    sPageButton:   "paginate_button page-item"
} );

/* Bootstrap paging button renderer */
DataTable.ext.renderer.pageButton.bootstrap = function ( settings, host, idx, buttons, page, pages ) {
    var api     = new DataTable.Api( settings );
    var classes = settings.oClasses;
    var lang    = settings.oLanguage.oPaginate;
    var aria = settings.oLanguage.oAria.paginate || {};
    var btnDisplay, btnClass, counter=0;

    var attach = function( container, buttons ) {
        var i, ien, node, button;
        var clickHandler = function ( e ) {
            e.preventDefault();
            if ( !$(e.currentTarget).hasClass('disabled') && api.page() != e.data.action ) {
                api.page( e.data.action ).draw( 'page' );
            }
        };

        for ( i=0, ien=buttons.length ; i<ien ; i++ ) {
            button = buttons[i];

            if ( Array.isArray( button ) ) {
                attach( container, button );
            }
            else {
                btnDisplay = '';
                btnClass = '';

                switch ( button ) {
                    case 'ellipsis':
                        btnDisplay = '&#x2026;';
                        btnClass = 'disabled';
                        break;

                    case 'first':
                        btnDisplay = lang.sFirst;
                        btnClass = button + (page > 0 ?
                            '' : ' disabled');
                        break;

                    case 'previous':
                        btnDisplay = lang.sPrevious;
                        btnClass = button + (page > 0 ?
                            '' : ' disabled');
                        break;

                    case 'next':
                        btnDisplay = lang.sNext;
                        btnClass = button + (page < pages-1 ?
                            '' : ' disabled');
                        break;

                    case 'last':
                        btnDisplay = lang.sLast;
                        btnClass = button + (page < pages-1 ?
                            '' : ' disabled');
                        break;

                    default:
                        btnDisplay = button + 1;
                        btnClass = page === button ?
                            'active' : '';
                        break;
                }

                if ( btnDisplay ) {
                    node = $('<li>', {
                            'class': classes.sPageButton + ' ' + btnClass,
                            'id': idx === 0 && typeof button === 'string' ?
                                settings.sTableId + '_' + button :
                                null
                        } )
                        .append( $('<a>', {
                                'href': '#',
                                'aria-controls': settings.sTableId,
                                'aria-label': aria[ button ],
                                'data-dt-idx': counter,
                                'tabindex': settings.iTabIndex,
                                'class': 'page-link'
                            } )
                            .html( btnDisplay )
                        )
                        .appendTo( container );

                    settings.oApi._fnBindAction(
                        node, {action: button}, clickHandler
                    );

                    counter++;
                }
            }
        }
    };

    // IE9 throws an 'unknown error' if document.activeElement is accessed
    // inside an iframe or frame. Try / catch the error to ignore it.
    var activeEl;

    try {
        activeEl = document.activeElement;
    }
    catch (e) {
        activeEl = document.body;
    }

    attach(
        $(host).empty().html('<ul class="pagination"/>').children('ul'),
        buttons
    );
};

// Javascript enhancements on table initialisation
$(document).on( 'init.dt', function ( e, ctx ) {
    if ( e.namespace !== 'dt' ) {
        return;
    }

    var api = new $.fn.dataTable.Api( ctx );

    // Length menu
    $( 'div.dataTables_length select', api.table().container() ).addClass('form-select form-select-sm');

    // Filtering input
    $( 'div.dataTables_filter input', api.table().container() ).addClass('form-control form-control-sm');

    // Info text
    $( 'div.dataTables_info', api.table().container() ).addClass('form-text text-muted');

    // Processing indicator
    $( 'div.dataTables_processing', api.table().container() ).addClass('card');
} );

return DataTable;
}));


