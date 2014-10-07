var qsparams = new Array();
$(document).ready(function () {
    var rex = document.URL.match(/[^\?&]+=[^\?&]+/g);
    for (var str in rex) {
        var param = decodeURIComponent(rex[str]).match(/[^\=]+/g);
        qsparams[param[0]] = param[1];
    }
    moment.lang($('#hdnculture').val());
    //moment(new Date()).format('dddd')
    getres();
});
var resx;
function getres() {
    $.ajax({
        type: "GET", url: "/gen/" + $('#resfile').val(), dataType: "json",
        success: function (json) {
            resx = json;//JSON.parse(json[0]);
            onloadpartII();
        }
    });
}
function onloadpartII() {
    var page = $('#pname').val();
    if (page == 'main')
        pagemain();
    else if (page == "tagged")
        pagetagged();
    else if (page == "profile")
        pageprofile();
    else if (page == "plupload")
        initpl();

}
function pagetagged() {
    var tag = qsparams["tag"];
    $('.authorDiv .spanbg').each(function () {
        $(this).html($(this).html().replace(tag, '<span class="color1">' + tag + "</span>"));
    });
}
function pagemain() {
    var storystate = parseInt($('#hdnstorystate').val());
    var firsttimecookie = $.cookie('frst');
    if (firsttimecookie == undefined) {
        $('.pfirsttime').show();
    }
    else {
        if (storystate == 1)
            $('.pregular').show();
        else if (storystate == 2)
            $('.pvoting').show();
    }
    if ($('.mainRBox').length > 0)
        $('.mainR').jscroll({
            loadingHtml: '<img src="/content/images/ajax-loader2.gif" alt="" /> ' + resx.loading + '...',
            padding: 20,
            callback: scrollentriescallback
            //nextSelector: 'a.jscroll-next:last',
            //, contentSelector: '.mainL'
        });
    var untilw = new Date($('#hdnnextexpiry').val());// new Date(2014, 1, 14, 23, 30);
    if (untilw < (new Date()))
        CountdownExpired();
    else
        $('#countdown' + storystate).countdown({
            until: untilw, onExpiry: ExpireV2
            , layout: '{mn} ' + resx.minutes + ', {sn} ' + resx.seconds// , {hn} {hl}, and {dn} {dl}'
        });

    if (qsparams["eid"] != undefined)
        $('.spanbg[eid="' + qsparams["eid"] + '"]').addClass('color3');

    checkmembervotes();
    if (storystate == 1 && $('#hdnmemberid').val() != "0")
        checklastentry();

    if ($('#hdnlastwinnerentryid').val() != '') {
        $("#secondpopup").dialog({
            modal: true,
            dialogClass: 'popup2', width: 760, height: 630
        });
        $('#tagentry').val($('#hdnlastwinnerentrytext').val());

        var lastelink = $('#hdnlastwinnerelink').val();
        var faceLink = "https://www.facebook.com/sharer/sharer.php?u="; //+ curl; //+ "&t=" + Url.Encode(ViewBag.Title);
        var twitLink = "https://twitter.com/intent/tweet?original_referer="; //+ curl + "&tw_p=tweetbutton&url=" + curl;
        $('.fb').attr('href', faceLink + escape(lastelink));
        $('.tw').attr('href', twitLink + escape(lastelink));

        $('.ui-dialog-titlebar,.ui-dialog-titlebar-close, .ui-dialog-title').hide();
    }
    checkdescvisibility();
    checknavbuttonsvisibility();
    if ($.cookie('loglastentry') != undefined && $.cookie('loglastentry') != '')
        $('.textarea').val($.cookie('loglastentry'));

    $(window).scrollTop(10);
    //setTimeout("testfade()", 3000);
    //$(window).scroll(function (event) {

    //    var pos = $(this).scrollTop();
    //    if (pos > lastScrollTop) {
    //    } else {
    //        if (pos == 0 && parseInt($('#currentpage').val()) > 1) {
    //            //console.log('hit top');
    //            //$(window).scrollTop(5);
    //            ajaxnavigate(true);
    //        }
    //    }
    //    lastScrollTop = pos;
    //});
    $(window).bind('mousewheel DOMMouseScroll', function (event) {
        if (event.originalEvent.wheelDelta > 0 || event.originalEvent.detail < 0) {
            var pos = $(this).scrollTop();
            if (pos > lastScrollTop) {
            } else {
                if (pos < 5 && parseInt($('#currentpage').val()) > 1) {
                    //console.log('hit top');
                    //$(window).scrollTop(5);
                    ajaxnavigate(true);
                }
            }
            lastScrollTop = pos;
        }
        else {
            // scroll down
        }
    });
    if ($('.spanbg').length < 10 && parseInt($('#currentpage').val()) > 1)
        ajaxnavigate(true);
    //"ajax-loader.gif"
    //$("#success").load("/not-here.php", function (response, status, xhr) {
    //    if (status == "error") {
    //        var msg = "Sorry but there was an error: ";
    //        $("#error").html(msg + xhr.status + " " + xhr.statusText);
    //    }
    //});
}
function scrollentriescallback() {
    modifyvotedentries();
}
function pageprofile() {
    if ($('#hdnmemberid').val() != "0") {
        var followcookie = $.cookie('flws');
        if (followcookie == undefined) {
            $.ajax({
                type: "GET", url: "/ajax/getfollows", dataType: "json",
                success: function (json) {
                    var cookie = JSON.stringify(json);
                    $.cookie("flws", cookie, { path: '/' });
                    if (cookie.indexOf($('#pmemberid').val()) != -1)
                        $('#fwbtn').html(resx.unfollow);
                }
            });
        }
        else {
            if (followcookie.indexOf($('#pmemberid').val()) != -1)
                $('#fwbtn').html(resx.unfollow);
        }
    }
}
function switchfollow(targetmemberid) {
    $.ajax({
        type: "GET", url: "/ajax/switchfollow", data: { targetmemberid: targetmemberid }, dataType: "json",
        success: function (json) {
            if (json == "1")
                $('#fwbtn').html(resx.unfollow);
            else
                $('#fwbtn').html(resx.follow);
            $.removeCookie('flws', { path: '/' });
        }
    });
}
var lastScrollTop = 5;
var isfirstscrollup = true;
function checkdescvisibility() {
    $('#storydesc').css('display', $('#currentpage').val() == "1" ? 'block' : 'none');
}
function ExpireV2() {
    document.location = document.location;
}
function CountdownExpired() {
    $('.info4').html('<img src="/content/images/ajax-loader2.gif"/> ' + resx.waitingfornextstep);
    setTimeout(function () { document.location = document.location; }, 4000);
}
function loadtagsright() {
    $('.mainR').load("/ajax/tags?storyid=" + $('#hdnstoryid').val());
}
function testfade() {//yeni yazilan entryler otomatik sayfada gorunsun
    $('#entryfades').prepend('<div class="axc" style="display:none">test new entry' + new Date() + '</div>');
    $('.axc').fadeIn("slow", function () { });
    setTimeout("testfade()", 3000);

}
function checkmembervotes(isforce) {
    if ($('#hdnmemberid').val() != "0" && ($.cookie("votes") == undefined || isforce))
        $.ajax({
            type: "GET", url: "/ajax/getmembervotes", dataType: "json",
            success: function (json) {
                var cookie = "";
                $(json).each(function () {
                    cookie += "~" + this.ItemID + '-' + !this.IsDislike;
                });
                $.cookie("votes", cookie, { path: '/' });
                modifyvotedentries();
            }
        });
    else
        modifyvotedentries();
}
function modifyvotedentries() {
    var cookie = $.cookie("votes");
    var elms = $('.mainRBox');
    if (cookie == undefined)
        return;
    for (var i = 0; i < elms.length; i++) {
        var entryid = $(elms[i]).attr('class').match(/entryvotes(\d+)/)[1];
        if (cookie.indexOf('~' + entryid + '-') != -1) {
            $(elms[i]).find('.likechoice').hide();
            var indexer = cookie.indexOf('~' + entryid + '-') + ('~' + entryid + '-').length;
            if (cookie.substring(indexer, indexer + 4) == "true")
                $(elms[i]).find('.marinRBoxLike2').show();
            else
                $(elms[i]).find('.marinRBoxDis2').show();
        }
    }
}
function checklastentry() {
    $.ajax({
        type: "GET", url: "/ajax/checklastentry", data: { storyid: $('#hdnstoryid').val() }, dataType: "json",
        success: function (json) {
            if (json != "0") {
                $('.alreadyposted').show();
                $('.notposted').hide();
                $('.info5').html('"' + json + '"');
            }
        }
    });
}
function VoteEntry(islike, entryid) {
    if ($('#hdnmemberid').val() == "0") {
        openlogger();
        return;
    }
    var cookie = $.cookie("votes");
    var storyentry = '~' + entryid + '-';
    if (cookie != undefined && cookie.indexOf(storyentry) != -1)
        return showdialog(resx.votedbefore);

    else
        $.ajax({
            type: "GET", url: "/ajax/voteentry", data: { storyid: $('#hdnstoryid').val(), entryid: entryid, islike: islike }, dataType: "json",
            success: function (json) {
                if (JSON.parse(json) == false)
                    return showdialog(resx.votedbefore);
                else {
                    //todooooo
                    var texter = "#l " + resx.likes + ", #d " + resx.dislikes;

                    var l = parseInt($('.entryvotes' + entryid + ' .span2').attr("l"));
                    var d = parseInt($('.entryvotes' + entryid + ' .span2').attr("d"));
                    if (islike)
                        $('.entryvotes' + entryid + ' .span2').html(texter.replace(/#l/, ++l).replace(/#d/, d));
                    else
                        $('.entryvotes' + entryid + ' .span2').html(texter.replace(/#l/, l).replace(/#d/, ++d));
                    checkmembervotes(true);
                }
            }
        });
}
function sendfeedback(entryid) {
    if ($('#hdnmemberid').val() == "0") {
        openlogger();
        return;
    }
    $.ajax({
        type: "GET", url: "/ajax/sendfeedback", data: { itemid: entryid, iteminfo: $('#txtComplain').val() }, dataType: "json",
        success: function (json) {
            //if (JSON.parse(json) == false)
            //    return showdialog(resx.votedbefore);
            showdialog(resx.complaintsent, null);
        }
    });
}

function ajaxnavigate(loadprev) {
    var pagecount = $('#pagecount').val();
    var currentpage = parseInt($('#currentpage').val());
    var storyid = $('#hdnstoryid').val();
    var minpage = parseInt($('#minpage').val());
    var maxpage = parseInt($('#maxpage').val());
    var pagetogo = loadprev ? minpage - 1 : maxpage + 1;
    $.ajax({
        type: "GET", url: "/ajax/getpageentries", data: { storyid: storyid, pageno: pagetogo }, dataType: "json",
        success: function (json) {
            var span = '<span class="mainentry spanbg" eid="#eid" pid="#pid" mid="#mid" mname="#mname" isanon="#isanon" sname="#sname" onclick="showinfo(this);">&nbsp;#txt</span>';
            var str = "";
            $(json).each(function () {
                if (this.isprgstart)
                    str += "<br/>";
                str += span.replace(/#eid/, this.eid).replace(/#pid/, this.pid).replace(/#mid/, this.mid).replace(/#isanon/, this.isanon)
                    .replace(/#mname/, this.mname).replace(/#txt/, this.text).replace(/#sname/, this.sname);
            });
            if (loadprev) {
                $('#contentbase').prepend(str);
                $(window).scrollTop(5);
            }
            else
                $('#contentbase').append(str);
        }
    });
    currentpage = pagetogo;
    $('#currentpage').val(currentpage);
    if (loadprev)
        $('#minpage').val(currentpage);
    else
        $('#maxpage').val(currentpage);
    checknavbuttonsvisibility();
    checkdescvisibility();
}

function checknavbuttonsvisibility() {
    var pagecount = parseInt($('#pagecount').val());
    var currentpage = parseInt($('#currentpage').val());
    var minpage = parseInt($('#minpage').val());
    var maxpage = parseInt($('#maxpage').val());
    if (currentpage == pagecount) {
        $('.golast, .next').hide();
    }
    if (currentpage == 1) {
        $('.gofirst').hide();
        $('.prev').hide();
    }
}
var sharelink;
function admintagger(entryid) {
    sharelink = $('#etlink > a').attr("href");

    $.ajax({
        url: "/ajax/admingettags", data: { entryid: entryid },
        success: function (json) {
            console.log(json.length);
            $("#genpopup").dialog('close');
            $("#secondpopup").dialog({
                modal: true,
                dialogClass: 'popup2', width: 760, height: 630
            });
            $('#tagentry').val($('.mainentry[eid=' + entryid + ']').html());
            $('#hdnlastwinnerentryid').val(entryid);
            $('.ui-dialog-titlebar,.ui-dialog-titlebar-close, .ui-dialog-title').hide();


            tags = new Array();
            for (var i = 0; i < json.length; i++) {
                var tag = { tag: json[i].tagtext, type: json[i].tagtype };
                tags.push(tag);
                $('#tagentry').val($('#tagentry').val().replace(tag.tag, '[' + tag.tag + ']'));
            }
            printtags();

            var faceLink = "https://www.facebook.com/sharer/sharer.php?u="; //+ curl; //+ "&t=" + Url.Encode(ViewBag.Title);
            var twitLink = "https://twitter.com/intent/tweet?original_referer="; //+ curl + "&tw_p=tweetbutton&url=" + curl;
            $('.fb').attr('href', faceLink + escape(sharelink));
            $('.tw').attr('href', twitLink + escape(sharelink));
        }
    });
}
var tags = new Array();
function AddTag(chk) {
    var tagtype = $('input[name=tagger]:checked').val();
    $(chk).prop('checked', false);
    if ($.trim($('#tagentry').textrange().text) == '')// || $('input[name=tagger]:checked').length == 0)
        return;
    var tag = { tag: $.trim($('#tagentry').textrange().text), type: tagtype };
    tag.tag = tag.tag.replace(/\[/, '').replace(/\]/, '');
    var exist = jQuery.grep(tags, function (n, i) { return n.tag == tag.tag; });
    if (exist.length == 1)
        return;
    tags.push(tag);
    $('#tagentry').textrange('replace', '[' + tag.tag + '] '); //.trigger('updateInfo').focus();
    printtags();
}
function printtags() {
    $('#tagged').html('');
    var tagcont = '<span onclick="ontagclick(this)">#tg, </span>';
    var html = '';
    for (var i = 0; i < tags.length; i++)
        html += tagcont.replace(/#tg/, tags[i].tag);
    $('#tagged').html(html);
    //alert($('#tagentry').textrange().text);
    //textrange().length
    //$(openerid).textrange('replace', $(this).val()); //.trigger('updateInfo').focus();
}
function PostTagsv2(elm) {
    $(elm).parent().parent().dialog('close');
    //if (tags.length == 0)
    //    return;
    var postdata = { storyid: $('#hdnstoryid').val(), entryid: $('#hdnlastwinnerentryid').val(), tags: tags }
    $.ajax({
        url: "/ajax/SaveTags", data: { tagsdata: JSON.stringify(postdata) },
        success: function (json) {
            //showdialog(json, function () { document.location = document.location; }); //$('textarea').val('');
            //close: function(ev, ui) { $(this).remove(); },
            document.location = document.location;

        }
    });
}
function RewriteText(ts) {
    var letters = "0123456789qwertyuiopasdfghjklzxcvbnm";
    var newts = "";
    ts = ts.toLowerCase().replace("ı", "i").replace("ğ", "g").replace("ş", "s").replace("ç", "c").replace("ü", "u").replace("ö", "o");
    for (var i = 0; i < ts.length; i++)
        if (letters.indexOf(ts.substring(i, i + 1)) == -1)
            newts += "-";
        else
            newts += ts.substring(i, i + 1);
    return newts;
}
function ontagclick(elm) {
    var tag = $(elm).html().replace(', ', '');
    var newlist = jQuery.grep(tags, function (n, i) { return n.tag != tag });
    $('#tagentry').val($('#tagentry').val().replace('[' + tag + ']', tag));
    tags = newlist;
    printtags();
}
function checkone(elm1) {
    if ($(elm1).prop('checked'))
        $('input[name=tagger]').each(function () { if (elm1 != this) $(this).prop('checked', false); });
}
function showinfo(elm) {
    var storyid = $('#hdnstoryid').length == 1 ? $('#hdnstoryid').val() : $(elm).attr('sid');
    showinfov2(storyid, $(elm).attr('pid'), $(elm).attr('eid'), $(elm).attr('mid'), $(elm).attr('mname'),
        $(elm).attr('isprg'), $('#hdnmemberrole').val(), $('.mainentry[eid=' + $(elm).attr('eid') + ']').text(), $(elm).attr('isanon'), $(elm).attr('sname'));
}
function showinfov2(storyid, pid, eid, mid, mname, isprg, mrole, etext, isanon, sname) {
    $.ajax({
        url: "/ajax/entryinfo", data: { storyid: storyid, pid: pid, eid: eid, mid: mid, mname: mname, isprg: isprg, mrole: mrole, etext: etext, isanon: isanon, sname: sname },
        success: function (html) {
            showdialog(html);
        }
    });
}
function storystarter(storyid) {
    var part2 = storyid == undefined ? "" : "?storyid=" + storyid;
    $.ajax({
        url: "/ajax/storystarter" + part2,
        success: function (html) {
            showdialog(html);
            $("#genpopup").css('width', '660px');
            if ($('#hdnprivateview').val() == 'False' && $('#hdnprivatewrite').val() == 'False')
                $('#slcviewers0').val("1");
            else if ($('#hdnprivateview').val() == 'True' && $('#hdnprivatewrite').val() == 'True')
                $('#slcviewers0').val("3");
            else if ($('#hdnprivateview').val() == 'False' && $('#hdnprivatewrite').val() == 'True')
                $('#slcviewers0').val("2");
            $('#slcviewers1,#slcviewers2').prop('disabled', $('#slcviewers0').val() == "1");
        }
    });
}
function appendoption(elm, value, text, selected) {
    if (selected)
        $(elm).append('<option value="' + value + '" selected="true">' + text + '</option>');
    else
        $(elm).append('<option value="' + value + '">' + text + '</option>');
}
function ChangeStoryView(storyid, type) {
    $('#slcviewers1,#slcviewers2').prop('disabled', type == 1);
    $.ajax({
        url: "/ajax/ChangeStoryView", data: { storyid: storyid, type: type},
        success: function (html) {
            //showdialog(html);
        }
    });
}
var addremovefn;

function AddRemoveStoryMember(storyid, type, isremove) {
    var memberid, slc;
    if (type == 3)
        slc = '#slcadmins';
    else if (type == 4)
        slc = '#slcviewers';
    if (isremove)
        slc += "2"
    else
        slc += "1";
    if ($(slc).val() == null)
        return;
    memberid = $(slc).val()[0];
    var membername = $(slc).children('option[value="' + memberid + '"]').text();
    if (isremove)
        //addremovefn = function () { $('#slcviewers1 option[value="' + memberid + '"]').remove(); $('#slcviewers1 option[value="' + memberid + '"]').remove(); };
        addremovefn = function () { $(slc).children('option[value="' + memberid + '"]').remove(); };
    else
        addremovefn = function () { appendoption($('#' + $(slc)[0].id.replace('1', '2')), memberid, membername, false); };
    $.ajax({
        url: "/ajax/AddRemoveStoryMember", data: { storyid: storyid, memberid: memberid, type: type, isremove: isremove },
        success: function (html) {
            //showdialog(html);
            if (html == "ok")
                addremovefn();
        }
    });
}
function saveuserstory() {
    if ($('#usrstoryfrm input[name="newstoryname"]').val() == '')
        return false;
    if ($('#usrstoryfrm input[name="newcharlimit"]').val() == '')
        return false;
    if ($('#usrstoryfrm input[name="newcounter1"]').val() == '')
        return false;
    if ($('#usrstoryfrm input[name="newcounter2"]').val() == '')
        return false;
    $('#usrstoryfrm').submit();
}
function limitnumeric(elm) {
    if ($(elm).val().match(/^\d*$/) == null) {
        $(elm).val($(elm).val().substring(0, $(elm).val().length - 1));
        return false;
    }
}
var authWindow;
function twitterconnect() {
    authWindow = window.open('about:blank');
    $.ajax({
        url: "/ajax/twitauth", data: {},
        success: function (json) {
            //alert(json);
            //window.open(json);
            authWindow.location.replace(json);
            //document.location = document.location;
            //close: function(ev, ui) { $(this).remove(); },

        }
    });
}
function linkedincallback(id, name, job) {
    $.ajax({
        url: "/ajax/linkedincallback", data: { id: id, name: name, job: job },
        success: function (json) {
            document.location = document.location;
        }
    });
}
function linkedincallback(id, name, job) {
    $.ajax({
        url: "/ajax/linkedincallback", data: { id: id, name: name, job: job },
        success: function (json) {
            document.location = document.location;
        }
    });
}
function DeleteTagRight(entrytagid) {
    $.ajax({
        url: "/ajax/admindeleteentrytag", data: { entrytagid: entrytagid },
        success: function (json) {
            alert(json);
            document.location = document.location;
        }
    });
}
function admindeleteentry(entryid) {
    $.ajax({
        url: "/ajax/admindeleteentry", data: { entryid: entryid },
        success: function (json) {
            alert(json);
            document.location = document.location;
        }
    });
}
function switchprg(entryid, setprg) {
    $.ajax({
        url: "/ajax/switchprg", data: { entryid: entryid, setprg: setprg },
        success: function (json) {
            alert(json);
        }
    });
}
function admineditentry(entryid) {
    $.ajax({
        url: "/ajax/admineditentrytext", data: { entryid: entryid, entrytext: $('#txtedittext').val() },
        success: function (json) {
            alert(json);
            $('.mainentry[eid=' + entryid + ']').html($('#txtedittext').val());
            $('#texteditor').toggle();
        }
    });
}
function showdialog(message, closefn) {
    $("#genpopup div div").html(message);
    $("#genpopup").dialog({
        title: resx.info,
        //dialogClass: 'popup3',
        resizable: false,
        modal: true,
        closeOnEscape: true,
        close: closefn,
        width: 550,
        open: function (event, ui) { $('.ui-widget-overlay').bind('click', function () { $("#genpopup").dialog('close'); }); }
    });
    //$('.ui-dialog-titlebar,.ui-dialog-titlebar-close, .ui-dialog-title').show();
    //$('.ui-widget-content').removeClass('ui-widget-content');
    //$('.ui-dialog .ui-dialog-content').css('background', 'white');
    $('.ui-dialog-titlebar,.ui-dialog-titlebar-close, .ui-dialog-title').hide();
    $('.ui-dialog .ui-dialog-content').css('background', 'none');
}
function firsttimeclick() {
    $.cookie('frst', '1', { expires: 9999 });
    //$.removeCookie('the_cookie');
    $('.pfirsttime').hide();
    $('.pregular').show();
}
function SubmitEntry() {
    if ($.trim($('.textarea').val()) == '')
        return;
    $.cookie('loglastentry', $('.textarea').val(), { path: '/' });
    $.ajax({
        url: "/ajax/postentry", data: { storyid: $('#hdnstoryid').val(), text: $.trim($('.textarea').val()), isanon: $('#chkanon').prop('checked') },
        success: function (json) {
            showdialog(json, function () { document.location = document.location; }); //$('textarea').val('');
            //close: function(ev, ui) { $(this).remove(); },

        }
    });
}
function switchmemberrole(memberid, makeadmin) {
    $.ajax({
        url: "/ajax/switchadmin", data: { memberid: memberid, makeadmin: makeadmin },
        success: function (json) {
            //showdialog(json, function () { document.location = document.location; }); //$('textarea').val('');
            alert(json);
            document.location = document.location;
            //close: function(ev, ui) { $(this).remove(); },

        }
    });
}
function ConnectFB() {
    FB.login(function (response) {
        if (response.status === 'connected')
            $.cookie('rememberme', '1', { expires: 9999 , path: '/' });
        LoginMember();
        //}, { scope: 'email,publish_stream' }); //,user_likes
    }, { scope: 'email' }); //,user_likes
}
function LoginMember() {
    if ($('#hdnmemberid').val() == "0" && $.cookie('rememberme') != undefined)
        FB.api('/me', function (response) {
            console.log(response.id);
            if (response.id != undefined)
                $.ajax({
                    url: "/ajax/FBLogin", data: { id: response.id, email: response.email, first_name: response.first_name, last_name: response.last_name }, success: function (json) {
                        //if ($('#firstpopup') &&  $('#firstpopup').dialog("isOpen")) {
                        //$('#firstpopup').dialog('close');
                        document.location = document.location;
                        //}
                    }
                });
            //username
        });
}
function Logout() {
    $.ajax({ url: "/ajax/logout", success: function (json) { $.removeCookie('rememberme', { path: '/' }); document.location = document.location; } });
}
function checklogin() {
    if ($('#hdnmemberid').val() == "0") {
        openlogger();
        return false;
    }
    else
        return true;
}
function openlogger() {
    $("#firstpopup").dialog({
        modal: true,
        width: 670,
        show: { effect: 'fade', duration: 500 },
        hide: { effect: 'fade', duration: 250 }
        //buttons: {        //    Ok: function() {        //        $( this ).dialog( "close" );        //    }        //}
    });
    $('.ui-dialog-titlebar,.ui-dialog-titlebar-close, .ui-dialog-title').hide();
    $('.ui-dialog .ui-dialog-content').css('background', 'none');
}
function limitchars(elm, limit, display, allowmultiple) {
    if (!checklogin()) {
        elm.value = elm.value.substring(0, elm.value.length - 1);
        return false;
    }
    if (allowmultiple == false && elm.value.indexOf('.') != -1) {
        if (elm.value.indexOf('.') + 1 < elm.value.length) {
            elm.value = elm.value.substring(0, elm.value.length - 1);
        }
    }
    else
        if (elm.value.length > limit) {
            elm.value = elm.value.substring(0, elm.value.length - (elm.value.length - limit));
        }
    if (display)
        $(display).html(limit - elm.value.length);
}