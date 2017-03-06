var _gameActive = false;

var gameController = (function () {
    var _gameTimerIntervalId;
    var _challengeTimerIntervalId;
    var _gameEndTime;
    var _challengeEndTime;
    var _challengeColors;
    var _usedWinnerGramIndexes = [];
    var _usedLoserGramIndexes = [];

    var _winnerGramImageArray = [
        "/images/fodrizzelgram.gif",
        "/images/gramhulk.jpg",
        "/images/ppap.png",
        "/images/gramphelps.jpg"
    ];

    var _loserGramImageArray = [
        "/images/gram-cauldron.png",
        "/images/walkinGram.jpg",
        "/images/gdebate.jpg"
    ];

    var _soundEffectHelper = {
        playSoundEffectBuzzer: function () {
            var soundEffectBuzzer = $('#soundEffectBuzzer');
            if (soundEffectBuzzer)
                soundEffectBuzzer[0].play();
        },
        playSoundEffectBeep: function () {
            var soundEffectBeep = $('#soundEffectBeep');
            if (soundEffectBeep)
                soundEffectBeep[0].play();
        }
    };

    var _gramHelper = {
        hideGram: function () {
            var $celebrationGramContainer = $('.js-celebrationGramContainer');
            $celebrationGramContainer.hide();
        },
        showGram: function (isWinner) {
            var $celebrationGramContainer = $('.js-celebrationGramContainer');
            var $celebrationGramImg = $('.js-celebrationGram');

            var image;
            var imageIndex = 0;
            if (isWinner) {
                if (_usedWinnerGramIndexes.length === _winnerGramImageArray.length) {
                    _usedWinnerGramIndexes = [];
                }

                do {
                    imageIndex = Math.floor(Math.random() * _winnerGramImageArray.length);
                } while ($.inArray(imageIndex, _usedWinnerGramIndexes) !== -1);

                _usedWinnerGramIndexes.push(imageIndex);
                image = _winnerGramImageArray[imageIndex];
            } else {
                if (_usedLoserGramIndexes.length === _loserGramImageArray.length) {
                    _usedLoserGramIndexes = [];
                }

                do {
                    imageIndex = Math.floor(Math.random() * _loserGramImageArray.length);
                } while ($.inArray(imageIndex, _usedLoserGramIndexes) !== -1);

                _usedLoserGramIndexes.push(imageIndex);

                image = _loserGramImageArray[imageIndex];
            }

            $celebrationGramImg.attr('src', image);
            $celebrationGramContainer.show();
        }
    }

    var _gamePlayHelper = {
        startGame: function() {
            var $gameRow = $('.js-gameRow[data-round=1]');
            $gameRow.addClass('js-active');
        },
        endGame: function (soundBuzzer) {
            if (_gameTimerIntervalId)
                clearInterval(_gameTimerIntervalId);
            if (_challengeTimerIntervalId)
                clearInterval(_challengeTimerIntervalId);

            _gameActive = false;

            _gramHelper.showGram(false);

            if (soundBuzzer)
                _soundEffectHelper.playSoundEffectBuzzer();
        },
        resetStats: function () {
            this.setScore('0');
            this.setSolvedCount('0');
            this.setFailedCount('0');

            var $challengeTimer = $('.js-challengeTimer');
            var $gameTimer = $('.js-gameTimer');
            this.setCountdownTimerText($challengeTimer, '00:00');
            this.setCountdownTimerText($gameTimer, '00:00');
        },
        setScore: function (value) {
            var $totalScore = $('.js-totalScore');
            var score = parseInt(value);

            if ($totalScore && score !== 'NaN')
                $totalScore.text(score);
        },
        setSolvedCount: function(value) {
            var $solvedCount = $('.js-solvedCount');
            var solved = parseInt(value);

            if ($solvedCount && solved !== 'NaN')
                $solvedCount.text(solved);
        },
        setFailedCount: function(value) {
            var $failedCount = $('.js-failedCount');
            var failed = parseInt(value);

            if ($failedCount && failed !== 'NaN')
                $failedCount.text(failed);
        },
        setTeamName: function (name) {
            var $teamNameContainer = $('.js-teamNameContainer');
            var $teamName = $('.js-teamName');
            if ($teamName)
                $teamName.text(name);

            $teamNameContainer.show();
        },
        setCountdownTimerText: function (element, value) {
            var $timer = element;
            if ($timer)
                $timer.text(value);
        },
        clearTeamNameInput: function () {
            var teamNameInput = $('#teamNameInput');

            if (teamNameInput)
                teamNameInput.val('');
        },
        getColorClass: function(color) {
            switch (color.toLowerCase()) {
            case 'red':
                return 'guessRed';
            case 'green':
                return 'guessGreen';
            case 'yellow':
                return 'guessYellow';
            case 'white':
                return 'guessWhite';
            case 'blue':
                return 'guessBlue';
            case 'black':
                return 'guessBlack';
            case 'orange':
                return 'guessOrange';
            case 'pink':
                return 'guessPink';
            default:
                return 'guessNoGuess';
            }
        },
        resetChallengeHistory: function () {
            var $challengeHistoryContainer = $('.js-challengeHistoryContainer');
            $challengeHistoryContainer.hide();

            var $challengeHistoryRows = $('.js-challengeHistory');

            $.each($challengeHistoryRows,
                function () {
                    var $element = $(this);

                    var id = parseInt($element.attr('data-id'));
                    if (id === 1) {
                        $element.removeClass('challengeHistoryPass');
                        $element.removeClass('challengeHistoryFail');
                        return;
                    }
                    $element.remove();
                });
        },
        resetLevelInformation: function () {
            this.setLevelValue('--');
            this.setColorCountValue('--');
            this.setDuplicatesValue('--');
            this.setAvailableGuessCount('--');
        },
        setLevelValue: function (value) {
            var $level = $('.js-level');
            $level.text(value);
        },
        setColorCountValue: function (value) {
            var $colors = $('.js-colors');
            $colors.text(value);
        },
        setDuplicatesValue: function (value) {
            var $duplicates = $('.js-duplicates');
            $duplicates.text(value);
        },
        setAvailableGuessCount: function(value) {
            var $availableGuesses = $('.js-availableGuesses');
            $availableGuesses.text(value);
        },
        resetChallengeRows: function() {
            var $gameRows = $('.js-gameRow');

            var iterator = 0;
            $.each($gameRows,
                function () {
                    var $gameRow = $(this);
                    if (iterator === 0) {
                        gameController.resetRow($gameRow);
                        gameController.setRound($gameRow, 1);
                        if (_gameActive)
                            $gameRow.addClass('js-active');
                    } else {
                        $gameRow.remove();
                    }
                    iterator++;
                });
        },
        updatePlayAreaContainerState: function(state) {
            var $playAreaContainer = $('.js-playAreaContainer');
            if (state === 'show') {
                $playAreaContainer.show();
            } else {
                $playAreaContainer.hide();
            }
        },
        updateViewingAreaContainerState: function (state) {
            var $viewingAreaContainer = $('.js-viewingAreaContainer');
            if (state === 'show') {
                $viewingAreaContainer.show();
            } else {
                $viewingAreaContainer.hide();
            }
        },
        updateLeaderboard: function(container, leaderboard) {
            var $container = container;
            var $leaderboardContainer = $container.find('.js-leaderboardContainer');
            var $leaderboardRows = $container.find('.js-leaderboardRow');

            var $seedRow;
            var iterator = 0;
            $.each($leaderboardRows,
                function() {
                    var $leaderboardRow = $(this);
                    if (iterator === 0) {
                        $seedRow = $leaderboardRow;
                    } else {
                        $leaderboardRow.remove();
                    }
                    iterator++;
                });

            iterator = 0;
            $.each(leaderboard,
                function(i, stat) {
                    var $leaderboardRank;
                    var $leaderboardName;
                    var $leaderboardValue;
                    if (iterator === 0) {
                        $leaderboardRank = $seedRow.find('.js-leaderboardRank');
                        $leaderboardName = $seedRow.find('.js-leaderboardName');
                        $leaderboardValue = $seedRow.find('.js-leaderboardValue');
                        $leaderboardRank.text(stat.Rank + '.');
                        $leaderboardName.text(stat.Name);
                        $leaderboardValue.text(stat.Value);
                    } else {
                        var $clone = $seedRow.clone();
                        $leaderboardRank = $clone.find('.js-leaderboardRank');
                        $leaderboardName = $clone.find('.js-leaderboardName');
                        $leaderboardValue = $clone.find('.js-leaderboardValue');
                        $leaderboardRank.text(stat.Rank + '.');
                        $leaderboardName.text(stat.Name);
                        $leaderboardValue.text(stat.Value);
                        $leaderboardContainer.append($clone);
                    }
                    iterator++;
                });
        }
    };

    var _timerHelper = {
        startGameTimer: function () {
            var now = new Date();
            var challengeTime = (_challengeEndTime - now) / 1000;
            _timerHelper.setChallengeTimerValue(challengeTime);

            if (typeof _gameEndTime !== 'undefined') {
                var gameTime = (_gameEndTime - now) / 1000;
                _timerHelper.setGameTimerValue(gameTime);

                if (_gameTimerIntervalId)
                    clearInterval(_gameTimerIntervalId);
                _gameTimerIntervalId = setInterval(function () {
                    --gameTime;
                    _timerHelper.setGameTimerValue(gameTime);

                    if ((_challengeEndTime - _gameEndTime) === 0) {
                        clearInterval(_gameTimerIntervalId);
                    }
                },
                    1000);

                $('.js-gameTimeContainer').show();
            }

            if (_challengeTimerIntervalId)
                clearInterval(_challengeTimerIntervalId);

            _challengeTimerIntervalId = setInterval(function() {
                --challengeTime;
                _timerHelper.setChallengeTimerValue(challengeTime);

                if ((_challengeEndTime - _gameEndTime) === 0) {
                    _timerHelper.setGameTimerValue(challengeTime);
                }
            },
                1000);
        },
        setGameTimerValue: function (timer) {
            var minutes = parseInt(timer / 60, 10);
            var seconds = parseInt(timer % 60, 10);

            minutes = minutes < 10 ? '0' + minutes : minutes;
            seconds = seconds < 10 ? '0' + seconds : seconds;

            var $gameTimer = $('.js-gameTimer');
            _gamePlayHelper.setCountdownTimerText($gameTimer, minutes + ':' + seconds);

            if (minutes === '00' && parseInt(seconds) < 10) {
                $gameTimer.addClass('finalCountdown');
            } else {
                $gameTimer.removeClass('finalCountdown');
            }

            if (minutes === '00' && seconds === '00') {
                _gamePlayHelper.endGame(true);
            }
        },
        setChallengeTimerValue: function (timer) {
            var minutes = parseInt(timer / 60, 10);
            var seconds = parseInt(timer % 60, 10);

            minutes = minutes < 10 ? '0' + minutes : minutes;
            seconds = seconds < 10 ? '0' + seconds : seconds;

            var $challengeTimer = $('.js-challengeTimer');
            _gamePlayHelper.setCountdownTimerText($challengeTimer, minutes + ':' + seconds);
            
            if (minutes === '00' && parseInt(seconds) < 10) {
                _soundEffectHelper.playSoundEffectBeep();
                $challengeTimer.addClass('finalCountdown');
            } else {
                $challengeTimer.removeClass('finalCountdown');
            }

            if (minutes === '00' && seconds === '00') {
                _gamePlayHelper.endGame(true);
            }
        }
    };

    return {
        startGame: function (gameEndTime, challengeEndTime) {
            _gameActive = true;
            if (gameEndTime !== 0) {
                _gameEndTime = new Date(gameEndTime);
            }
            _challengeEndTime = new Date(challengeEndTime);
            this.updateGameControlState('hide');
            _timerHelper.startGameTimer();
            _gamePlayHelper.startGame();
        },
        updateChallengeTimer: function (challengeTime) {
            _challengeEndTime = new Date(challengeTime);
            _timerHelper.startGameTimer();
        },
        updateGameControlState: function (state) {
            var $gameControlContainer = $('.js-gameControlContainer');
            if (state === 'hide') {
                $gameControlContainer.hide();
            } else {
                $gameControlContainer.show();
            }
        },
        updateLevelInformation: function(level, availableColorCount, duplicatesAllowed, availableGuessCount) {
            _gamePlayHelper.setLevelValue(level);
            _gamePlayHelper.setColorCountValue(availableColorCount);
            _gamePlayHelper.setAvailableGuessCount(availableGuessCount);

            var duplicateText = '--';
            if (duplicatesAllowed) {
                duplicateText = 'Y';
            } else {
                duplicateText = 'N';
            }
            _gamePlayHelper.setDuplicatesValue(duplicateText);
        },
        updateReadyButtonState: function(state) {
            var $readyBtn = $('.js-ready');
            if (state === 'enable') {
                $readyBtn.removeAttr('style');
                $readyBtn.attr('disabled', false);
            } else {
                $readyBtn.attr('style', 'display: none;');
                $readyBtn.attr('disabled', true);
            }
        },
        endGame: function (soundBuzzer) {
            _gamePlayHelper.endGame(soundBuzzer);
        },
        resetGame: function () {
            this.updateStartGameButtonState('disabled');
            this.updateGameControlsDisplay('hide');
            this.updateResetGameButtonState('disabled');

            _gamePlayHelper.clearTeamNameInput();
        },
        updateStats: function (score, solved, failed) {
            _gamePlayHelper.setScore(score);
            _gamePlayHelper.setSolvedCount(solved);
            _gamePlayHelper.setFailedCount(failed);
        },
        updateConnectedUsersCount: function (count) {
            var connectedUsersText = $('#connectedUsersText');
            if (connectedUsersText)
                connectedUsersText.text(count);
        },
        showTeamNameSelectionModal: function () {
            var teamNameSelectionModal = $('#teamNameSelectionModal');
            var $btn = $('.js-registerTeam');
            $btn.attr('disabled', false);

            if (teamNameSelectionModal)
                teamNameSelectionModal.modal('show');
        },
        setTeamName: function (name) {
            _gamePlayHelper.setTeamName(name);
        },
        updateColorClass: function (element) {
            var $element = element;
            var currentIndex = parseInt($element.attr('data-colorId'));

            var challengeColorLength = _challengeColors.length;
            var newIndex = currentIndex + 1;
            if (newIndex > challengeColorLength - 1) {
                newIndex = 0;
            }

            var lastColor = '';
            if (currentIndex >= 0) {
                lastColor = _challengeColors[currentIndex];
            }
            var lastColorClass = _gamePlayHelper.getColorClass(lastColor);

            var newColor = _challengeColors[newIndex];
            var newColorClass = _gamePlayHelper.getColorClass(newColor);

            $element.removeClass(lastColorClass);
            $element.addClass(newColorClass);

            $element.attr('data-colorId', newIndex);
        },
        setPossibleColors: function(possibleColors) {
            _challengeColors = possibleColors;
        },
        resetPlayArea: function() {
            _gamePlayHelper.resetChallengeHistory();
            _gamePlayHelper.resetLevelInformation();
            _gamePlayHelper.resetStats();
            _gamePlayHelper.resetChallengeRows();
            _gramHelper.hideGram();
        },
        endGameForPlayer: function() {
            var $activeGameRow = $('.js-active');
            $activeGameRow.removeClass('js-active');
            _gameActive = false;
        },
        resetRow: function(element) {
            var $gameRow = element;
            var $guesses = $gameRow.find('.js-guess');
            var $results = $gameRow.find('.js-result');

            $.each($guesses,
                function () {
                    var $guess = $(this);
                    $guess.attr('class', 'js-guess guess guessNoGuess');
                    $guess.attr('data-colorId', '-1');
                });

            $.each($results,
                function () {
                    var $result = $(this);
                    $result.attr('class', 'js-result result noResult');
                });

            return $gameRow;
        },
        clearChallengeArea: function() {
            _gamePlayHelper.resetChallengeRows();
        },
        buildGuessString: function(element) {
            var $element = element;
            var $gameRow = $element.closest('.js-gameRow');
            var $jsGuesses = $gameRow.find('.js-guess');
            var $celebrationGramContainer = $('.js-celebrationGramContainer');
            $celebrationGramContainer.hide();

            var guessString = '';
            var firstColor = true;

            $.each($jsGuesses,
                function () {
                    var $element = $(this);

                    if (firstColor) {
                        firstColor = false;
                    } else {
                        guessString += ',';
                    }

                    if ($element.hasClass('guessRed')) {
                        guessString += 'red';
                    } else if ($element.hasClass('guessGreen')) {
                        guessString += 'green';
                    } else if ($element.hasClass('guessYellow')) {
                        guessString += 'yellow';
                    } else if ($element.hasClass('guessBlack')) {
                        guessString += 'black';
                    } else if ($element.hasClass('guessBlue')) {
                        guessString += 'blue';
                    } else if ($element.hasClass('guessWhite')) {
                        guessString += 'white';
                    } else if ($element.hasClass('guessOrange')) {
                        guessString += 'orange';
                    } else if ($element.hasClass('guessPink')) {
                        guessString += 'pink';
                    }
                });

            return guessString;
        },
        setRound: function(element, round) {
            var $gameRow = element;
            var $round = $gameRow.find('.js-round');
            if (round === null || round === undefined) {
                round = parseInt($round.text()) + 1;
            }
            $gameRow.attr('data-round', round);
            $round.text(round);
        },
        celebrationGram: function(isWinner) {
            _gramHelper.showGram(isWinner);
        },
        createChallengeHistory: function(id, level, points, guesses, time, solved) {
            var $challengeHistoryContainer = $('.js-challengeHistoryContainer');
            $challengeHistoryContainer.show();

            var $challengeHistory = $('.js-challengeHistory[data-id=1]');
            var $newChallengeHistoryRow = $challengeHistory;
            if (id !== 1) {
                $newChallengeHistoryRow = $challengeHistory.clone();
            }

            var $level = $newChallengeHistoryRow.find('.js-historyLevel');
            $level.text(level);

            var $points = $newChallengeHistoryRow.find('.js-historyPoints');
            $points.text(points);

            var $guesses = $newChallengeHistoryRow.find('.js-historyGuesses');
            $guesses.text(guesses);

            var minutes = parseInt(time / 60, 10);
            var seconds = parseInt(time % 60, 10);

            minutes = minutes < 10 ? '0' + minutes : minutes;
            seconds = seconds < 10 ? '0' + seconds : seconds;

            var $time = $newChallengeHistoryRow.find('.js-historyTime');
            $time.text(minutes + ':' + seconds);

            if (solved) {
                $newChallengeHistoryRow.addClass('challengeHistoryPass');
                $newChallengeHistoryRow.removeClass('challengeHistoryFail');
            } else {
                $newChallengeHistoryRow.addClass('challengeHistoryFail');
                $newChallengeHistoryRow.removeClass('challengeHistoryPass');
            }

            $newChallengeHistoryRow.attr('data-id', id);

            var $challengeHistoryRecords = $('.js-challengeHistoryRecords');
            $challengeHistoryRecords.prepend($newChallengeHistoryRow);
        },
        showAnswer: function(answer) {
            var $modal = $('#correctAnswerModal');

            var $answers = $('.js-answer');

            var iterator = 0;
            $.each($answers,
                function() {
                    var $answerRow = $(this);
                    var color = answer[iterator];
                    var colorClass = _gamePlayHelper.getColorClass(color);

                    $answerRow.addClass(colorClass);
                    iterator++;
                });

            $modal.modal('show');
        },
        showPlayArea: function() {
            _gamePlayHelper.updateViewingAreaContainerState('hide');
            _gamePlayHelper.updatePlayAreaContainerState('show');
        },
        showViewingArea: function() {
            _gamePlayHelper.updatePlayAreaContainerState('hide');
            _gamePlayHelper.updateViewingAreaContainerState('show');
        },
        updateViewingArea: function(scoreLeaderboard,
            roundsCompletedLeaderboard,
            fastestLeaderboard,
            fewestLeaderboard) {
            var $leaderboardScoreContainer = $('.js-leaderboardScoreContainer');
            var $leaderboardRoundsContainer = $('.js-leaderboardRoundsContainer');
            var $leaderboardFastestContainer = $('.js-leaderboardFastestContainer');
            var $leaderboardFewestContainer = $('.js-leaderboardFewestContainer');

            _gamePlayHelper.updateLeaderboard($leaderboardScoreContainer, scoreLeaderboard);
            _gamePlayHelper.updateLeaderboard($leaderboardRoundsContainer, roundsCompletedLeaderboard);
            _gamePlayHelper.updateLeaderboard($leaderboardFastestContainer, fastestLeaderboard);
            _gamePlayHelper.updateLeaderboard($leaderboardFewestContainer, fewestLeaderboard);
        }
    };
}());