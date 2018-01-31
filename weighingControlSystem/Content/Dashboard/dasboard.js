
$(document).ready(function () {
    $.get('/WebsiteCommon/GetMenuList', function (data) {
        debugger;
        var subMenu1 = '<ul class="dropdown-menu">';
        var subMenu2 = '</ul>';
        var menuData = '';
            
        for (var i = 0; i < data.length; i++) {
            //MainMenu add
            menuData += '<a href="#" tabindex='+(i+1)+' class="dropdown-toggle" data-toggle="dropdown"> <li class="menu">';
            if (data[i].mainMenu == 'Account') {
                menuData += '<i class="material-icons" style="font-size:12px !important;">perm_identity</i>';
            } else if (data[i].mainMenu == 'Upload')
            { menuData += '<i class="material-icons" style="font-size:12px !important;">present_to_all</i>' }
            else if (data[i].mainMenu == 'Report')
            { menuData += '<i class="material-icons" style="font-size:12px !important;">subject</i>' }
            else if (data[i].mainMenu == 'Masters')
            { menuData += '<i class="material-icons" style="font-size:12px !important;">library_books</i>' } else { menuData += '<i class="material-icons" style="font-size:12px !important;">library_add</i>' }

            menuData += '<span>' + data[i].mainMenu + '</span>' +
                            '</a>';
            //Child
            if (data[i].subMenu.length > 0) {
                var menu = '<ul class="dropdown-menu multi-level" >';
                for (var j = 0; j < data[i].menu.length; j++) {

                    if (data[i].subMenu.length > 0 && data[i].menu[j].menuUrl=="No") {
                        menu += '<li  class="dropdown-submenu">' +
                                   '<a href="javascript:void(0);" class="menu-toggle waves-effect waves-block">' +
                                       '<span>' + data[i].menu[j].menuName + '</span>' +
                                   '</a>';
                    } else {
                        menu += '<li>' +
                              '<a href=' + data[i].menu[j].menuUrl + ' class=" waves-effect waves-block">' +
                                  '<span>' + data[i].menu[j].menuName + '</span>' +
                              '</a>';
                    }

                    //Sub Menu
                    if (data[i].subMenu.length > 0 && data[i].menu[j].menuUrl == "No") {
                        menu += subMenu1;
                        for (var k = 0; k < data[i].subMenu.length; k++) {
                            if (data[i].subMenu[k].menu_fkid == data[i].menu[j].pkid) {
                                menu += '<li ><a href=' + data[i].subMenu[k].subMenuUrl + '  class="waves-effect waves-block"><span>' + data[i].subMenu[k].subMenuName + '</span></a></li>'
                            }

                        }
                        menu += subMenu2
                    }
                    menu += '</li>';
                    subMenu = '';
                    debugger;

                }
            } else {
                var menu = '<ul class="dropdown-menu" >';
                for (var j = 0; j < data[i].menu.length; j++) {
                    debugger;
                    menu += '<li>' +
                               '<a href=' + data[i].menu[j].menuUrl + ' class=" waves-effect waves-block">' +
                                   '<span>' + data[i].menu[j].menuName + '</span>' +
                               '</a>' +
                          ' </li>';

                    debugger;

                }


            }
            menuData += menu + '</ul></li>';
            menu = '';

        }
        $('#toptMenuDynamic').html(menuData);


    }).done(function () {
        $.AdminBSB.leftSideBar.activate(); $.AdminBSB.rightSideBar.activate(); $.AdminBSB.navbar.activate();
        $.AdminBSB.dropdownMenu.activate();
    })


    $('.redirectToListOnClick').click(function () {
        debugger;
        window.location = $(this).find('.number').attr('data-redirect');

    })



})






//leftMenuDynamic

