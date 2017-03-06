(function ($, gameController) {
    // Declare a proxy to reference the hub.
    var gamePlayHub = $.connection.gamePlayHub;

    // Start the connection.
    $.connection.hub.start();

    $(document).on('click', '.js-gameRow.js-active .js-submit',
        function () {
            var $btn = $(this);
            $btn.attr('disabled', true);
            var guessString = gameController.buildGuessString($btn);
            gamePlayHub.server.submitGuess(guessString);
        });

    function CreateNewRow(element) {
        var $gameRow = element;
        var $cloneGameRow = $gameRow.clone();
        var $gameRows = $('.js-gameRows');

        $gameRow.removeClass('js-active');

        var $resetGameRow = gameController.resetRow($cloneGameRow);
        gameController.setRound($cloneGameRow);
        $gameRows.append($resetGameRow);
    }


    $(document)
        .on('click',
            '.js-startGame',
            function() {
                var $btn = $(this);
                $btn.attr('disabled', true);

                gameController.resetPlayArea();

                gamePlayHub.server.startGame();
            });

    $(document)
        .on('click',
            '.js-ready',
            function() {
                var $btn = $(this);
                $btn.attr('disabled', true);

                gameController.resetPlayArea();

                gamePlayHub.server.readyUpPlayer();
            });

    $(document).on('click', '.js-gameRow.js-active .js-guess',
        function () {
            var $element = $(this);
            var $gameRow = $element.closest('.js-gameRow');

            gameController.updateColorClass($element);

            var $btn = $gameRow.find('.js-submit');
            var $noGuesses = $gameRow.find('.guessNoGuess');
            if ($noGuesses.length > 0) {
                $btn.attr('disabled', true);
            } else {
                $btn.attr('disabled', false);
            }
        });

    $(document)
        .on('click',
            '.js-registerTeam',
            function() {
                var $btn = $(this);
                $btn.attr('disabled', true);
                var teamName = $('.js-teamNameInput').val();

                gamePlayHub.server.registerTeam(teamName);
            });

    gamePlayHub.client.updateTeamName = function (teamName) {
        gameController.setTeamName(teamName);
    }

    gamePlayHub.client.enableTeamRegistration = function (message) {
        if (typeof message !== 'undefined') {
            alert(message);
        }
        gameController.showTeamNameSelectionModal();
    }

    gamePlayHub.client.updateChallengeColors = function(possibleColors) {
        gameController.setPossibleColors(possibleColors);
    }

    gamePlayHub.client.updateGame = function (round, response, gameOver, winner, score, updatedChallengeEndTime, solvedCount, failedCount, level, availableColorCount, duplicatesAllowed, availableGuessCount) {
        var $gameRow = $('.js-gameRow[data-round=' + round + ']');
        var $results = $gameRow.find('.js-result');

        gameController.updateChallengeTimer(updatedChallengeEndTime);
        gameController.updateLevelInformation(level, availableColorCount, duplicatesAllowed, availableGuessCount);

        var index = 0;
        $.each($results,
            function () {
                var $element = $(this);
                var responseValue = response[index];
                if (responseValue == 2) {
                    $element.removeClass('noResult');
                    $element.addClass('correctColorAndPosition');
                }
                else if (responseValue == 1) {
                    $element.removeClass('noResult');
                    $element.addClass('correctColor');
                }
                index++;
            });

        gameController.updateStats(score, solvedCount, failedCount);

        if (!gameOver && !winner) {
            CreateNewRow($gameRow);
            return;
        }

        setTimeout(function () {
            gameController.clearChallengeArea();
        }, 2000);

        gameController.celebrationGram(winner);
    }

    gamePlayHub.client.createChallengeHistory = function(id, level, points, guesses, time, solved) {
        gameController.createChallengeHistory(id, level, points, guesses, time, solved);
    }

    gamePlayHub.client.enableReadyButton = function () {
        gameController.showPlayArea();
        gameController.updateGameControlState('show');
        gameController.updateReadyButtonState('enable');
    }

    gamePlayHub.client.startGame = function (gameTime, challengeTime, level, availableColors, duplicatesAllowed, availableGuessCount) {
        gameController.startGame(gameTime, challengeTime);
        gameController.updateLevelInformation(level, availableColors, duplicatesAllowed, availableGuessCount);
    }

    gamePlayHub.client.playerClockGameOver = function() {
        gameController.endGameForPlayer();
    }

    gamePlayHub.client.enableStartButton = function () {
        var $gameControlContainer = $('.js-gameControlContainer');
        $gameControlContainer.show();

        gameController.showPlayArea();

        $('.js-startGame').show();
        $('.js-startGame').attr('disabled', false);
    }

    gamePlayHub.client.showAnswer = function(answer) {
        gameController.showAnswer(answer);
    }

    gamePlayHub.client.updateConnectedCount = function (users, players) {
        $('.js-connectedUsers').text(users);
        $('.js-connectedPlayers').text(players);
        $('.js-connectedPlayerCount').show();
    }

    gamePlayHub.client.updateViewingArea = function(scoreLeaderboard,
        roundsLeaderboard,
        fastestLeaderboard,
        fewestLeaderboard) {
        gameController.updateViewingArea(scoreLeaderboard, roundsLeaderboard, fastestLeaderboard, fewestLeaderboard);
    }

    gamePlayHub.client.addFifthColor = function() {
        var $gameRow = $('.js-gameRow[data-round=1]');
        var $guessResultContainer = $gameRow.find('.js-guessResultContainer');
        var $results = $gameRow.find('.js-result:first');

        var $clone;
        $guessResultContainer.addClass('guessResultsContainerFive');
        if ($gameRow.find('.js-result').length < 5) {
            $clone = $results.clone();
            $guessResultContainer.append($clone);
        }

        var $userGuessContainer = $gameRow.find('.js-userGuessContainer');
        $userGuessContainer.addClass('userGuessContainerFive');
        var $guesses = $gameRow.find('.js-guess:first');

        if ($gameRow.find('.js-guess').length < 5) {
            $clone = $guesses.clone();
            $userGuessContainer.append($clone);
        }
    }
}(window.jQuery, gameController));