import { Record, Union } from "../client/.fable/fable-library.3.1.2/Types.js";
import { lambda_type, list_type, record_type, string_type, class_type, union_type } from "../client/.fable/fable-library.3.1.2/Reflection.js";
import { toString, newGuid } from "../client/.fable/fable-library.3.1.2/Guid.js";
import { printf, toText } from "../client/.fable/fable-library.3.1.2/String.js";
import { FSharpResult$2 } from "../client/.fable/fable-library.3.1.2/Choice.js";

export class Domain_CardValue extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Zero", "Halve", "One", "Two", "Three", "Five", "Eight", "Thirtheen", "Twenty", "Fourty", "Hundred", "Coffee", "Stop", "IDontKnow"];
    }
}

export function Domain_CardValue$reflection() {
    return union_type("Shared.Domain.CardValue", [], Domain_CardValue, () => [[], [], [], [], [], [], [], [], [], [], [], [], [], []]);
}

export class Domain_Card extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Card"];
    }
}

export function Domain_Card$reflection() {
    return union_type("Shared.Domain.Card", [], Domain_Card, () => [[["Item", Domain_CardValue$reflection()]]]);
}

export function Domain_CardModule_create(value) {
    return new Domain_Card(0, value);
}

export class Domain_Player extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Player"];
    }
}

export function Domain_Player$reflection() {
    return union_type("Shared.Domain.Player", [], Domain_Player, () => [[["id", class_type("System.Guid")], ["name", string_type]]]);
}

export function Domain_PlayerModule_create(name) {
    return new Domain_Player(0, newGuid(), name);
}

export class Domain_GameId extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["GameId"];
    }
}

export function Domain_GameId$reflection() {
    return union_type("Shared.Domain.GameId", [], Domain_GameId, () => [[["Item", string_type]]]);
}

export function Domain_GameIdModule_create(id) {
    return new Domain_GameId(0, id);
}

export function Domain_GameIdModule_extract(_arg1) {
    const id = _arg1.fields[0];
    return id;
}

export class Domain_Game extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Game"];
    }
}

export function Domain_Game$reflection() {
    return union_type("Shared.Domain.Game", [], Domain_Game, () => [[["id", Domain_GameId$reflection()], ["admin", Domain_Player$reflection()]]]);
}

export function Domain_GameModule_create(admin) {
    let copyOfStruct;
    const gameId = Domain_GameIdModule_create((copyOfStruct = newGuid(), toString(copyOfStruct, "N")));
    return new Domain_Game(0, gameId, admin);
}

export function Domain_GameModule_extract(_arg1) {
    const id = _arg1.fields[0];
    const admin = _arg1.fields[1];
    return [id, admin];
}

export function Domain_GameModule_$007CGetGameAdmin$007C_$007C(game) {
    const admin = Domain_GameModule_extract(game)[1];
    return admin;
}

export function Domain_GameModule_$007CGetGameId$007C_$007C(game) {
    const id = Domain_GameModule_extract(game)[0];
    return id;
}

export class Domain_InGameState extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Beginning", "InRound", "DisplayResult"];
    }
}

export function Domain_InGameState$reflection() {
    return union_type("Shared.Domain.InGameState", [], Domain_InGameState, () => [[], [], []]);
}

export class Domain_PlayedCard extends Record {
    constructor(Player, Card) {
        super();
        this.Player = Player;
        this.Card = Card;
    }
}

export function Domain_PlayedCard$reflection() {
    return record_type("Shared.Domain.PlayedCard", [], Domain_PlayedCard, () => [["Player", Domain_Player$reflection()], ["Card", Domain_Card$reflection()]]);
}

export class Domain_InGameModel extends Record {
    constructor(Game, State, Players, PlayedCards) {
        super();
        this.Game = Game;
        this.State = State;
        this.Players = Players;
        this.PlayedCards = PlayedCards;
    }
}

export function Domain_InGameModel$reflection() {
    return record_type("Shared.Domain.InGameModel", [], Domain_InGameModel, () => [["Game", Domain_Game$reflection()], ["State", Domain_InGameState$reflection()], ["Players", list_type(Domain_Player$reflection())], ["PlayedCards", list_type(Domain_PlayedCard$reflection())]]);
}

export class Domain_GameModel extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Start", "InGame"];
    }
}

export function Domain_GameModel$reflection() {
    return union_type("Shared.Domain.GameModel", [], Domain_GameModel, () => [[], [["Item", Domain_InGameModel$reflection()]]]);
}

export class Domain_Msg extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["CreateGame", "EndGame", "JoinGame", "LeaveGame", "StartRound", "FinishRound", "PlayCard"];
    }
}

export function Domain_Msg$reflection() {
    return union_type("Shared.Domain.Msg", [], Domain_Msg, () => [[["admin", Domain_Player$reflection()]], [], [["player", Domain_Player$reflection()]], [["player", Domain_Player$reflection()]], [], [], [["card", Domain_Card$reflection()]]]);
}

export const Route_builder = (() => {
    const clo1 = toText(printf("/api/%s/%s"));
    return (arg10) => {
        const clo2 = clo1(arg10);
        return clo2;
    };
})();

export class Api_IPokerApi extends Record {
    constructor(getState, createGame, endGame, joinGame, leaveGame, startRound, finishRound, playCard) {
        super();
        this.getState = getState;
        this.createGame = createGame;
        this.endGame = endGame;
        this.joinGame = joinGame;
        this.leaveGame = leaveGame;
        this.startRound = startRound;
        this.finishRound = finishRound;
        this.playCard = playCard;
    }
}

export function Api_IPokerApi$reflection() {
    return record_type("Shared.Api.IPokerApi", [], Api_IPokerApi, () => [["getState", lambda_type(Domain_GameId$reflection(), class_type("Microsoft.FSharp.Control.FSharpAsync`1", [union_type("Microsoft.FSharp.Core.FSharpResult`2", [Domain_GameModel$reflection(), string_type], FSharpResult$2, () => [[["ResultValue", Domain_GameModel$reflection()]], [["ErrorValue", string_type]]])]))], ["createGame", lambda_type(Domain_Player$reflection(), class_type("Microsoft.FSharp.Control.FSharpAsync`1", [union_type("Microsoft.FSharp.Core.FSharpResult`2", [Domain_GameModel$reflection(), string_type], FSharpResult$2, () => [[["ResultValue", Domain_GameModel$reflection()]], [["ErrorValue", string_type]]])]))], ["endGame", lambda_type(Domain_GameId$reflection(), lambda_type(Domain_Player$reflection(), class_type("Microsoft.FSharp.Control.FSharpAsync`1", [union_type("Microsoft.FSharp.Core.FSharpResult`2", [Domain_GameModel$reflection(), string_type], FSharpResult$2, () => [[["ResultValue", Domain_GameModel$reflection()]], [["ErrorValue", string_type]]])])))], ["joinGame", lambda_type(Domain_GameId$reflection(), lambda_type(Domain_Player$reflection(), class_type("Microsoft.FSharp.Control.FSharpAsync`1", [union_type("Microsoft.FSharp.Core.FSharpResult`2", [Domain_GameModel$reflection(), string_type], FSharpResult$2, () => [[["ResultValue", Domain_GameModel$reflection()]], [["ErrorValue", string_type]]])])))], ["leaveGame", lambda_type(Domain_GameId$reflection(), lambda_type(Domain_Player$reflection(), lambda_type(Domain_Player$reflection(), class_type("Microsoft.FSharp.Control.FSharpAsync`1", [union_type("Microsoft.FSharp.Core.FSharpResult`2", [Domain_GameModel$reflection(), string_type], FSharpResult$2, () => [[["ResultValue", Domain_GameModel$reflection()]], [["ErrorValue", string_type]]])]))))], ["startRound", lambda_type(Domain_GameId$reflection(), lambda_type(Domain_Player$reflection(), class_type("Microsoft.FSharp.Control.FSharpAsync`1", [union_type("Microsoft.FSharp.Core.FSharpResult`2", [Domain_GameModel$reflection(), string_type], FSharpResult$2, () => [[["ResultValue", Domain_GameModel$reflection()]], [["ErrorValue", string_type]]])])))], ["finishRound", lambda_type(Domain_GameId$reflection(), lambda_type(Domain_Player$reflection(), class_type("Microsoft.FSharp.Control.FSharpAsync`1", [union_type("Microsoft.FSharp.Core.FSharpResult`2", [Domain_GameModel$reflection(), string_type], FSharpResult$2, () => [[["ResultValue", Domain_GameModel$reflection()]], [["ErrorValue", string_type]]])])))], ["playCard", lambda_type(Domain_GameId$reflection(), lambda_type(Domain_Player$reflection(), lambda_type(Domain_Card$reflection(), class_type("Microsoft.FSharp.Control.FSharpAsync`1", [union_type("Microsoft.FSharp.Core.FSharpResult`2", [Domain_GameModel$reflection(), string_type], FSharpResult$2, () => [[["ResultValue", Domain_GameModel$reflection()]], [["ErrorValue", string_type]]])]))))]]);
}

