var Mentula = function Mentula() {
    var _this = this;
    this.host = "localhost";
    this.port = "9922";
    this.password = "";
    this.loginDialog = document.querySelector("#loginDialog");
    this.loginServer = document.querySelector("#serverHost");
    this.loginPassword = document.querySelector("#serverPassword");
    this.loginConnect = document.querySelector("#serverConnect");
    this.serverCount = 0;
    this.loginConnect.addEventListener('click', function () {
        if (_this.loginPassword.value !== "" && _this.loginServer.value !== "") {
            if (_this.loginServer.value.indexOf(':')) {
                _this.host = _this.loginServer.value.split(':')[0];
                _this.port = _this.loginServer.value.split(':')[1];
            } else
                _this.host = _this.loginServer.value;
            _this.password = _this.loginPassword.value;
            _this.loginDialog.close();
            _this.init();
        }
    });
    if (navigator.userAgent.toLowerCase().indexOf('firefox') > -1) {
        dialogPolyfill.registerDialog(this.loginDialog);
    }
    this.loginDialog.showModal();
}

/**************************
 *          INIT          *
 **************************/

Mentula.prototype.init = function () {
    this.baseURI = `http://${this.host}:${this.port}`;
    this.connectionURI = `${this.baseURI}/signalr`;
    this.hubURI = `${this.baseURI}/signalr/hubs`;
    this.mapImageBaseURI = "./Content/Images/";
    this.serverConnection = null;
    this.currentTab = null;
    this.extra = new MentulaExtra();

    var hubsScript = document.createElement('script');
    hubsScript.setAttribute("src", this.hubURI);
    hubsScript.setAttribute("async", "false");
    var _this = this;
    hubsScript.onload = function () {
        $.connection.hub.url = _this.connectionURI;
        _this.serverConnection = $.connection.ServerHub;
        _this.initEvents();
        $.connection.hub.start().done(function () {
            _this.serverConnection.server.loginEvent(_this.password);
        });
    }
    document.head.append(hubsScript);
}

Mentula.prototype.initEvents = function () {
    var _this = this;
    this.serverConnection.client.LoginEvent = function (result, Token) {
        _this.LoginCallback(result, Token);
    }
    this.serverConnection.client.GetServerListEvent = function (result) {
        _this.GetServerListCallback(result);
    }
    this.serverConnection.client.GetPlayersListEvent = function (result) {
        _this.GetPlayerListCallback(result);
    }
    this.serverConnection.client.KickPlayerEvent = function (result) {
        _this.KickPlayerCallback(result);
    }
    this.serverConnection.client.GetServerStatusEvent = function (result) {
        _this.GetServerStatusCallback(result);
    }
    this.serverConnection.client.SkipServerEvent = function (result) {
        _this.SkipServerCallback(result);
    }
    this.serverConnection.client.LoadBanListEvent = function (result) {
        _this.LoadBanListCallback(result);
    }
    this.serverConnection.client.BanPlayerEvent = function (result) {
        _this.BanPlayerCallback(result);
    }
    this.serverConnection.client.UnBanPlayerEvent = function (result) {
        _this.UnBanPlayerCallback(result);
    }
    this.serverConnection.client.LoadPlaylistsEvent = function(result) {
        _this.LoadPlayListsCallback(result);
    }
    this.serverConnection.client.ChangePlaylistEvent = function(result) {
        _this.ChangePlaylistCallback(result);
    }
    this.serverConnection.client.LoadServerLogEvent = function(result) {
        _this.LoadServerLogCallback(result);
    }
    this.serverConnection.client.NotifyServerChangeEvent = function() {
        _this.serverConnection.server.getServerListEvent();
    }
    window.setInterval(function () {
        _this.updateCurrentTab();
    }, 5000);
};

Mentula.prototype.InitNewServerTab = function (server) {
    var _this = this;
    //Create new tab link element for header
    var newTabLink = $('<a></a>').text(server["Name"]);
    newTabLink.attr("href", `#${server["Name"].replace(/\s/g, '-')}`);
    newTabLink.attr("data-index", server["Index"]);
    newTabLink.addClass("mdl-layout__tab");
    $('.mdl-layout__tab-bar').append(newTabLink);

    newTabLink[0].addEventListener("click",
        function () {
            _this.tabChangedEvent(newTabLink[0]);
        });
    //Create new tab content element
    var newTab = $("<section></section>").append($("<div></div>").addClass("page-content"));
    newTab.addClass("mdl-layout__tab-panel");
    newTab.attr("id", server["Name"].replace(/\s/g, '-'));
    $('.mdl-layout__content').append(newTab);
    newTabLink[0].tabContent = newTab[0];

    //Clone the template for tab content and append it
    var tabContents = document.getElementById('ServerContentsTemplate').content.cloneNode(true);
    var newTabElm = document.getElementById(`${server["Name"].replace(/\s/g, '-')}`);
    newTabElm.append(tabContents);

    //Hook the skip button in the new tab content
    var skipButton = newTabElm.querySelector('.mentula-skip');
    skipButton.addEventListener('click', function () {
        _this.serverConnection.server.skipServerEvent(server["Index"]);
    });

    //Hook the stop button
    var stopButton = newTabElm.querySelector('.mentula-stop');
    stopButton.addEventListener('click',
        function() {
            //_this.StopServerEvent();
        });

    //Hook the restart button
    var restartButton = newTabElm.querySelector('.mentula-restart');
    restartButton.addEventListener('click',
        function() {
            _this.RestartServerEvent();
        });

    //Hook banlist refresh button
    var banrefresh = newTabElm.querySelector('.banlist-container h4 i');
    banrefresh.addEventListener('click',
        function() {
            _this.LoadBanListEvent();
        });

    //Hook server log refresh button
    var logrefresh = newTabElm.querySelectorAll('.right-container h4 i')[0];
    logrefresh.addEventListener('click',
        function() {
            _this.LoadServerLogEvent();
        });

}

/**************************
 *       FUNCTIONS        *
 **************************/


Mentula.prototype.LoginEvent = function (Password) {
    this.serverConnection.server.loginEvent(Password);
}

Mentula.prototype.updateCurrentTab = function () {
    this.tabChangedEvent(this.currentTab, true);
}

Mentula.prototype.KickPlayerEvent = function (PlayerName) {
    this.serverConnection.server.kickPlayerEvent(this.currentTab.dataset["index"], PlayerName);
}

Mentula.prototype.tabChangedEvent = function (tabElement, force) {

    //Move the player table updating to its own function
    var forced = force || false;
    if (this.currentTab !== tabElement || forced) {
        //Rewriting MDL Code cause they suck donkey balls.
        document.querySelectorAll('.mdl-layout__tab-bar a').forEach(function(button) {
            button.className = "mdl-layout__tab";
        });
        tabElement.classList.add('is-active');
        document.querySelectorAll('.mdl-layout__tab-panel').forEach(function(tab) {
            tab.className = "mdl-layout__tab-panel";
        });
        tabElement.tabContent.classList.add('is-active');
        this.currentTab = tabElement;
        var serverIndex = tabElement.dataset["index"];
        this.serverConnection.server.getCurrentPlayersEvent(serverIndex);
        this.serverConnection.server.getServerStatusEvent(serverIndex);
        this.serverConnection.server.loadPlaylistsEvent();
    }
}

Mentula.prototype.LoadBanListEvent = function() {
    this.serverConnection.server.loadBanListEvent(this.currentTab.dataset["index"]);
}

Mentula.prototype.UnBanPlayerEvent = function(PlayerName) {
    this.serverConnection.server.unBanPlayerEvent(this.currentTab.dataset["index"], PlayerName);
}

Mentula.prototype.BanPlayerEvent = function(PlayerName) {
    this.serverConnection.server.banPlayerEvent(this.currentTab.dataset["index"], PlayerName);
}

Mentula.prototype.LoadPlaylistsEvent = function() {
    this.serverConnection.server.loadPlaylistsEvent();
}

Mentula.prototype.ChangePlaylistEvent = function(PlayList) {
    this.serverConnection.server.changePlaylistEvent(this.currentTab.dataset["index"], PlayList);
}

Mentula.prototype.LoadServerLogEvent = function() {
    this.serverConnection.server.loadServerLogEvent(this.currentTab.dataset["index"]);
}

Mentula.prototype.RestartServerEvent = function() {
    this.serverConnection.server.restartServerEvent(this.currentTab.dataset["index"]);
}

/**************************
 *       CALL BACKS       *
 **************************/

Mentula.prototype.LoginCallback = function (result, Token) {
    if (result === "Success") {
        this.serverConnection.qs = { "Token": Token };
        this.serverConnection.server.getServerListEvent();
    } else {
        alert("Login Failed, Refresh page to try again.");
    }
}

Mentula.prototype.GetServerListCallback = function (result) {
    $('.mdl-layout__tab-bar')[0].innerHTML = "";
    $('.mdl-layout__content')[0].innerHTML = "";
    var servers = JSON.parse(result);
    this.serverCount = servers.length;
    var _this = this;
    servers.forEach(function (server) {
        _this.InitNewServerTab(server);
    });
    $('.mdl-layout__tab:first-of-type').addClass("is-active");
    $('.mdl-layout__tab-panel:first-of-type').addClass("is-active");
    this.tabChangedEvent($('.mdl-layout__tab:first-of-type')[0]);
    componentHandler.upgradeElement(document.body);
};

Mentula.prototype.GetServerStatusCallback = function (result) {
    var status = JSON.parse(result);
    var tabContent = this.currentTab.tabContent;
    if (this.serverCount !== parseInt(status["ServerCount"])) {
        this.serverConnection.server.getServerListEvent();
    } else {
        //Set background of current map
        tabContent.querySelector('.status-card .mdl-card__title').style.background =
            `url(${this.mapImageBaseURI}${status["CurrentMap"]}.png)`;
        //Update current game state and map name
        tabContent.querySelector('.status-card .mdl-card__title .mdl-card__title-text').innerText =
            `${this.extra.localizeStatus(status["GameState"])}: ${this.extra.localizeMapName(status["CurrentMap"])}`;
        //Display current variant
        tabContent.querySelector('[data-elm="currentvariant"]').innerText =
            `${status["CurrentName"]} on ${this.extra.localizeMapName(status["CurrentMap"])}`;
        //Display next variant
        tabContent.querySelector('[data-elm="nextvariant"]').innerText =
            `${status["NextName"]} on ${this.extra.localizeMapName(status["NextMap"])}`;
    }
}

Mentula.prototype.GetPlayerListCallback = function (result) {
    var currentPanel = document.querySelector('.mdl-layout__tab-panel.is-active');
    var playerTable = currentPanel.querySelector('.player-table tbody');
    playerTable.querySelectorAll('[data-name]').forEach(function(row) {
        row.setAttribute("data-keep", "0");
    });
    var players = JSON.parse(result);
    var _this = this;
    players.forEach(function (player) {
        if (playerTable.querySelector(`[data-name="${player["Name"]}`) === null) {
            var newRow = document.querySelector('#PlayerTableRowTemplate').content.cloneNode(true);
            newRow.querySelectorAll('.mdl-data-table__cell--non-numeric')[0].style.background =
                `url(${player["EmblemURL"]}`;
            newRow.querySelector('.player-row').setAttribute('data-keep', "1");
            newRow.querySelector('.player-row').setAttribute("data-name", player["Name"]);
            newRow.querySelectorAll('.mdl-data-table__cell--non-numeric')[1].innerText = player["Name"];
            newRow.querySelectorAll('.mdl-data-table__cell--non-numeric')[2].innerText = player["Team"];
            newRow.querySelectorAll('.material-icons')[0].addEventListener('click',
                function () {
                    _this.BanPlayerEvent(player["Name"]);
                });
            newRow.querySelectorAll('.material-icons')[1].addEventListener('click',
                function() {
                    _this.KickPlayerEvent(player["Name"]);
                });
            playerTable.append(newRow);
        } else {
            playerTable.querySelector(`[data-name="${player["Name"]}`).setAttribute("data-keep", "1");
        }
    });
    playerTable.querySelectorAll('[data-keep="0"]').forEach(function (row) {
        row.outerHTML = "";
    });
};

Mentula.prototype.KickPlayerCallback = function (result) {
    Console.log("Player Kicked Successfully");
}

Mentula.prototype.SkipServerCallback = function (result) {
    Console.log("Server skipped successfully");
}

Mentula.prototype.LoadBanListCallback = function(result) {
    var bannedPlayers = JSON.parse(result);
    var currentPanel = document.querySelector('.mdl-layout__tab-panel.is-active');
    var banList = currentPanel.querySelector('.banlist');
    banList.innerHTML = "";
    var _this = this;
    bannedPlayers.forEach(function(bannedPlayer) {
        var newItem = document.querySelector('#BanListRowTemplate').content.cloneNode(true);
        newItem.querySelector('.banlist-name').innerText = bannedPlayer;
        newItem.querySelector('i').addEventListener('click',
            function() {
                _this.UnBanPlayerEvent(bannedPlayer);
            });
        banList.append(newItem);
    });
}

Mentula.prototype.UnBanPlayerCallback = function(result) {
    var _this = this;
    window.setTimeout(function () {
        _this.LoadBanListEvent();
    }, 3000);
}

Mentula.prototype.BanPlayerCallback = function(result) {
    var _this = this;
    window.setTimeout(function () {
        _this.LoadBanListEvent();
    }, 3000);
}

Mentula.prototype.LoadPlayListsCallback = function(result) {
    var playlists = JSON.parse(result);
    var currentPanel = document.querySelector('.mdl-layout__tab-panel.is-active');
    var playList = currentPanel.querySelector('.playlist-container ul');
    playList.innerHTML = "";
    var _this = this;
    playlists.forEach(function(playlist) {
        var newItem = document.querySelector('#PlaylistRowTemplate').content.cloneNode(true);
        newItem.querySelector('.mdl-list__item-primary-content').innerText = playlist;
        newItem.querySelector('.mdl-list__item').addEventListener('click',
            function() {
                _this.ChangePlaylistEvent(playlist);
            });
        playList.append(newItem);
    });
}

Mentula.prototype.ChangePlayListCallback = function(result) {
}

Mentula.prototype.LoadServerLogCallback = function(result) {
    var Log = JSON.parse(result);
    var currentPanel = document.querySelector('.mdl-layout__tab-panel.is-active');
    var logElement = currentPanel.querySelector('.log');
    logElement.innerHTML = "";
    Log.forEach(function(logLine) {
        var newRow = document.createElement('p');
        newRow.innerText = logLine;
        newRow.className = "log-item";
        logElement.append(newRow);
    });
}






document.addEventListener('DOMContentLoaded', function () {
    window["Mentula"] = new Mentula();
});