//This shit is so crude.

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
        _this.ChangePlayListCallback(result);
    }
    this.serverConnection.client.LoadServerLogEvent = function(result) {
        _this.LoadServerLogCallback(result);
    }
    this.serverConnection.client.NotifyServerChangeEvent = function() {
        _this.serverConnection.server.getServerListEvent();
    }
    this.serverConnection.client.StopServerEvent = function() {
        _this.StopServerCallback(result);
    }
    this.serverConnection.client.LoadVIPListEvent = function(result) {
        _this.LoadVIPListCallback(result);
    }
    this.serverConnection.client.AddVIPPlayerEvent = function(result) {
        _this.AddVIPPlayerCallback(result);
    }
    this.serverConnection.client.RemoveVIPPlayerEvent = function(result) {
        _this.RemoveVIPPlayerCallback(result);
    }
    window.setInterval(function () {
        _this.updateCurrentState();
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
        function () {
            _this.StopServerEvent();
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
    //Hook banlist refresh button
    var viprefresh = newTabElm.querySelector('.vip-container h4 i');
    viprefresh.addEventListener('click',
        function () {
            _this.LoadVIPListEvent();
        });
    //Hook server log refresh button
    var logrefresh = newTabElm.querySelectorAll('.right-container h4 i')[0];
    logrefresh.addEventListener('click',
        function() {
            _this.LoadServerLogEvent();
        });

    //Hook the Banlist add button
    var banAdd = newTabElm.querySelector('.ban-add button');
    newTabElm.querySelector('.ban-add .mdl-textfield input').id = `${server.Index}-ban-add`;
    newTabElm.querySelector('.ban-add .mdl-textfield label').setAttribute('for', `${server.Index}-ban-add`);
    banAdd.addEventListener('click',
        function() {
            var player = newTabElm.querySelector('.ban-add .mdl-textfield input').value;
            if (player !== "") {
                newTabElm.querySelector('.ban-add .mdl-textfield input').value = "";
                _this.BanPlayerEvent(player);
            }
        });

    //Hook the VIPlist add button
    var vipAdd = newTabElm.querySelector('.vip-add button');
    newTabElm.querySelector('.vip-add .mdl-textfield input').id = `${server.Index}-vip-add`;
    newTabElm.querySelector('.vip-add .mdl-textfield label').setAttribute('for', `${server.Index}-vip-add`);
    vipAdd.addEventListener('click',
        function () {
            var player = newTabElm.querySelector('.vip-add .mdl-textfield input').value;
            if (player !== "") {
                newTabElm.querySelector('.ban-add .mdl-textfield input').value = "";
                _this.AddVIPPlayerEvent(player);
            }
        });

    //Hook the Server Privacy
    var serverPrivacy = newTabElm.querySelector('.server-privacy input');
    serverPrivacy.id = `${server.Index}-privacy`;
    newTabElm.querySelector('.server-privacy label').setAttribute('for', `${server.Index}-privacy`);
    serverPrivacy.addEventListener('change', function () {
        _this.SetPrivacyEvent(this.dataset["value"]);
    });

    //Hook the Server forced biped
    var serverBiped = newTabElm.querySelector('.server-biped input');
    serverBiped.id = `${server.Index}-biped`;
    newTabElm.querySelector('.server-biped label').setAttribute('for', `${server.Index}-biped`);
    serverBiped.addEventListener('change', function() {
        _this.SetForcedBipedEvent(this.dataset["value"]);
    });

    //Hook the Server max Players
    var serverMaxPlayers = newTabElm.querySelector('.server-max-players input');
    serverMaxPlayers.id = `${server.Index}-max-players`;
    newTabElm.querySelector('.server-max-players label').setAttribute('for', `${server.Index}-max-players`);
    serverMaxPlayers.addEventListener('change', function() {
        _this.SetMaxPlayersEvent(this.dataset["value"]);
    });

    //Hook the Xdelay Timer
    var serverXDelay = newTabElm.querySelector('.server-xdelay-timer input');
    serverXDelay.id = `${server.Index}-xdelay-timer`;
    newTabElm.querySelector('.server-xdelay-timer label').setAttribute('for', `${server.Index}-xdelay-timer`);
    serverXDelay.addEventListener('change', function () {
        if (this.validity.valid) {
            _this.SetXDelayTimer(this.value);
        }
    });

    //Hook the gay ass battle rifle shit that actually does nothing.
    var serverBattleRifle = newTabElm.querySelector('.server-br-fix input');
    serverBattleRifle.id = `${server.Index}-br-fix`;
    newTabElm.querySelector('.server-br-fix label').setAttribute('for', `${server.Index}-br-fix`);
    serverBattleRifle.addEventListener('change', function () {
        if (this.validity.valid) {
            _this.SetBRFixEvent(this.value);
        }
    });

    //Hook the freeze lobby
    var serverFreeze = newTabElm.querySelector('.server-freeze-lobby input');
    serverFreeze.id = `${server.Index}-freeze-lobby`;
    newTabElm.querySelector('.server-freeze-lobby').setAttribute('for', `${server.Index}-freeze-lobby`);
    serverFreeze.addEventListener('change', function () {
            _this.FreezeLobbyEvent(this.checked.toString());
    });

    //Hook the Force Start Game
    var serverForce = newTabElm.querySelector('.server-force-start');
    serverForce.id = `${server.Index}-force-start`;
    serverForce.addEventListener('click', function() {
        _this.ForceStartLobbyEvent();
    });

    componentHandler.upgradeElements(newTabElm);
    //document.querySelectorAll('.mdl-textfield').forEach(function (a) { componentHandler.upgradeElement(a) });
}

/**************************
 *       FUNCTIONS        *
 **************************/

Mentula.prototype.updateCurrentState = function(){
    if (this.currentTab !== undefined) {
        this.serverConnection.server.getCurrentPlayersEvent(this.currentServer);
        this.serverConnection.server.getServerStatusEvent(this.currentServer);
    }
}

Mentula.prototype.LoginEvent = function (Password) {
    this.serverConnection.server.loginEvent(Password);
}

Mentula.prototype.KickPlayerEvent = function (PlayerName) {
    this.serverConnection.server.kickPlayerEvent(this.currentServer, PlayerName);
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
        this.currentServer = tabElement.dataset["index"];
        this.LoadPlaylistsEvent();
        this.LoadBanListEvent();
        this.LoadVIPListEvent();
        this.LoadServerLogEvent();
    }
}

Mentula.prototype.LoadBanListEvent = function() {
    this.serverConnection.server.loadBanListEvent(this.currentServer);
}

Mentula.prototype.UnBanPlayerEvent = function(PlayerName) {
    this.serverConnection.server.unBanPlayerEvent(this.currentServer, PlayerName);
}

Mentula.prototype.BanPlayerEvent = function(PlayerName) {
    this.serverConnection.server.banPlayerEvent(this.currentServer, PlayerName);
}

Mentula.prototype.TimeoutPlayerEvent = function(PlayerName) {
    this.serverConnection.server.timeoutPlayerEvent(this.currentServer, PlayerName);
}

Mentula.prototype.LoadPlaylistsEvent = function() {
    this.serverConnection.server.loadPlaylistsEvent();
}

Mentula.prototype.ChangePlaylistEvent = function(PlayList) {
    this.serverConnection.server.changePlaylistEvent(this.currentServer, PlayList);
}

Mentula.prototype.LoadServerLogEvent = function() {
    this.serverConnection.server.loadServerLogEvent(this.currentServer);
}

Mentula.prototype.RestartServerEvent = function() {
    if (confirm(
        "ARE YOU SURE YOU WANT TO DO THIS, THIS WILL RESTART THE SERVICE. THIS ACTION WILL BE LOGGED")
    ) {
        this.serverConnection.server.restartServerEvent(this.currentServer);
    }
}

Mentula.prototype.StopServerEvent = function() {
    if (confirm(
        "ARE YOU SURE YOU WANT TO DO THIS, IF YOU CLICK OKAY THE SERVER OWNER WILL HAVE TO MANUALLY START THE SERVICE. THIS ACTION WILL BE LOGGED")
    ) {
        this.serverConnection.server.stopServerEvent(this.currentServer);
    }
}

Mentula.prototype.LoadVIPListEvent = function() {
    this.serverConnection.server.loadVIPListEvent(this.currentServer);
}

Mentula.prototype.AddVIPPlayerEvent = function(playerName) {
    this.serverConnection.server.addVIPPlayerEvent(this.currentServer, playerName);
}

Mentula.prototype.RemoveVIPPlayerEvent = function(playerName) {
    this.serverConnection.server.removeVIPPlayerEvent(this.currentServer, playerName);
}

Mentula.prototype.FreezeLobbyEvent = function(state) {
    this.serverConnection.server.freezeLobbyEvent(this.currentServer, state);
}

Mentula.prototype.ForceStartLobbyEvent = function() {
    this.serverConnection.server.forceStartLobbyEvent(this.currentServer);
}

Mentula.prototype.SetPrivacyEvent = function(privacy) {
    this.serverConnection.server.setPrivacyEvent(this.currentServer, privacy);
}

Mentula.prototype.SetForcedBipedEvent = function(biped) {
    this.serverConnection.server.setForcedBipedEvent(this.currentServer, biped);
}

Mentula.prototype.SetMaxPlayersEvent = function(playerCount) {
    this.serverConnection.server.setMaxPlayersEvent(this.currentServer, playerCount);
}

Mentula.prototype.SetXDelayTimer = function(xDelayTime) {
    this.serverConnection.server.setXDelayTimer(this.currentServer, xDelayTime);
}

Mentula.prototype.SetBRFixEvent = function(value) {
    this.serverConnection.server.setBRFixEvent(this.currentServer, value);
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
    if (window.location.href.indexOf('Block') === -1) { //Used for design testing.
        $('.mdl-layout__tab-bar')[0].innerHTML = "";
        $('.mdl-layout__content')[0].innerHTML = "";
    }
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

        if (tabContent.querySelector('.server-freeze-lobby input').checked !== (status["LobbyRunning"] !== "true")) {
            tabContent.querySelector('.server-freeze-lobby').MaterialSwitch
                .setState(status["LobbyRunning"] !== "True");
        }
        if (tabContent.querySelector('.server-privacy').MaterialSelect.selectedItem() !== status["Privacy"])
            tabContent.querySelector('.server-privacy').MaterialSelect.setSelectedItem(status["Privacy"]);
        if (tabContent.querySelector('.server-biped').MaterialSelect.selectedItem() !== status["ForcedBiped"])
            tabContent.querySelector('.server-biped').MaterialSelect.setSelectedItem(status["ForcedBiped"]);
        if (tabContent.querySelector('.server-max-players').MaterialSelect.selectedItem() !== status["MaxPlayers"])
            tabContent.querySelector('.server-max-players').MaterialSelect.setSelectedItem(status["MaxPlayers"]);
        if (tabContent.querySelector('.server-xdelay-timer input').value !== status["XDelayTimer"])
            tabContent.querySelector('.server-xdelay-timer').MaterialTextfield.change(status["XDelayTimer"]);
        if (tabContent.querySelector('.server-br-fix input').value !== status["BRFix"])
            tabContent.querySelector('.server-br-fix').MaterialTextfield.change(status["BRFix"]);
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
            newRow.querySelectorAll('.material-icons')[2].addEventListener('click',
                function() {
                    _this.TimeoutPlayerEvent(player["Name"]);
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
        var newItem = document.querySelector('#ListRowTemplate').content.cloneNode(true);
        newItem.querySelector('.list-name').innerText = bannedPlayer;
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

Mentula.prototype.StopServerCallback = function(result) {

}

Mentula.prototype.RestartServerCallback = function(result) {

}

Mentula.prototype.LoadVIPListCallback = function(result) {
    var vipPlayers = JSON.parse(result);
    var currentPanel = document.querySelector('.mdl-layout__tab-panel.is-active');
    var vipList = currentPanel.querySelector('.viplist');
    vipList.innerHTML = "";
    var _this = this;
    vipPlayers.forEach(function (vipPlayer) {
        var newItem = document.querySelector('#ListRowTemplate').content.cloneNode(true);
        newItem.querySelector('.list-name').innerText = vipPlayer;
        newItem.querySelector('i').addEventListener('click',
            function () {
                _this.RemoveVIPPlayerEvent(vipPlayer);
            });
        vipList.append(newItem);
    });
}

Mentula.prototype.AddVIPPlayerCallback = function(result) {
    var _this = this;
    window.setTimeout(function () {
        _this.LoadVIPListEvent();
    }, 3000);
}

Mentula.prototype.RemoveVIPPlayerCallback = function(result) {
    var _this = this;
    window.setTimeout(function () {
        _this.LoadVIPListEvent();
    }, 3000);
}





document.addEventListener('DOMContentLoaded', function () {
    window["Mentula"] = new Mentula();
});