if (typeof window['blazorJavascript'] === 'undefined') {
    let blazorJavascript = {};

    blazorJavascript.interopMarkerSymbol = Symbol();

    blazorJavascript.obtainPrototype = function(o) {
        console.error("BlazorJavascript initialization error, you may be calling too early!")
    };

    blazorJavascript.checkIsNull = function(val) {
        return val === null;
    };

    blazorJavascript.checkIsUndefined = function(val) {
        return typeof val === 'undefined';
    };

    blazorJavascript.constructString = function(val) {
        if (!val) {
            return null;
        }

        return blazorJavascript.wrapForInterop(val + "");
    };

    blazorJavascript.constructBoolean = function(val) {
        return blazorJavascript.wrapForInterop(!!val);
    };

    blazorJavascript.constructNumberFromDouble = function(val) {
        return blazorJavascript.wrapForInterop(parseFloat(val));
    };

    blazorJavascript.constructNumberFromFloat = function(val) {
        return blazorJavascript.wrapForInterop(parseFloat(val));
    };

    blazorJavascript.constructNumberFromInt = function(val) {
        return blazorJavascript.wrapForInterop(parseInt(val, 10));
    };

    blazorJavascript.constructPositiveInfinity = function() {
        return blazorJavascript.wrapForInterop(Infinity);
    };

    blazorJavascript.constructNegativeInfinity = function() {
        return blazorJavascript.wrapForInterop(-Infinity);
    };

    blazorJavascript.constructNaN = function() {
        return blazorJavascript.wrapForInterop(NaN);
    };

    blazorJavascript.isNaN = function(val) {
        return isNaN(blazorJavascript.unwrap(val));
    };

    blazorJavascript.isInfinity = function(val) {
        let unwrappedVal = blazorJavascript.unwrap(val);
        return unwrappedVal === Number.POSITIVE_INFINITY || unwrappedVal === Number.NEGATIVE_INFINITY;
    };

    blazorJavascript.isPositiveInfinity = function(val) {
        let unwrappedVal = blazorJavascript.unwrap(val);
        return unwrappedVal === Number.POSITIVE_INFINITY;
    };

    blazorJavascript.isNegativeInfinity = function(val) {
        let unwrappedVal = blazorJavascript.unwrap(val);
        return unwrappedVal === Number.NEGATIVE_INFINITY;
    };

    blazorJavascript.isFinite = function(val) {
        return isFinite(blazorJavascript.unwrap(val));
    };

    blazorJavascript.isInteger = function(val) {
        return Number.isInteger(blazorJavascript.unwrap(val));
    };

    blazorJavascript.getWindow = function() {
        return blazorJavascript.wrapForInterop(window);
    };

    blazorJavascript.wrapForInterop = function(result) {
        if (blazorJavascript.isInteropWrapped(result)) {
            return result;
        }

        return {
            'reference': result,
            'marker': blazorJavascript.interopMarkerSymbol
        };
    };

    blazorJavascript.getPrototypeChain = function(o) {
        const chain = [];
        let lastPrototype = Object.getPrototypeOf(o);

        while (lastPrototype !== null) {
            chain.push(lastPrototype);
            lastPrototype = Object.getPrototypeOf(lastPrototype);
        }

        return chain;
    };

    blazorJavascript.unwrap = function(o) {
        if (blazorJavascript.isInteropWrapped(o)) {
            return o.reference;
        }

        return o;
    };

    blazorJavascript.getterFunction = function (o, k) {
        if (blazorJavascript.checkIsNull(o) || blazorJavascript.checkIsUndefined(o)) {
            return blazorJavascript.wrapForInterop(undefined);
        }

        let r = blazorJavascript.isInteropWrapped(o) ? o.reference : o;

        if (typeof r[k] === "undefined") {
            return blazorJavascript.wrapForInterop(undefined);
        }

        return blazorJavascript.wrapForInterop(r[k]);
    };

    blazorJavascript.evalFunction = function (o) {
        return blazorJavascript.unwrap(o);
    };

    blazorJavascript.setterFunction = function (o, k, v) {
        if (blazorJavascript.checkIsNull(o) || blazorJavascript.checkIsUndefined(o)) {
            return;
        }

        let r = blazorJavascript.isInteropWrapped(o) ? o.reference : o;

        r[k] = blazorJavascript.unwrap(v);
    };

    blazorJavascript.arrayItemAtIndex = function(o, i) {
        let unwrapped = blazorJavascript.unwrap(o);
        let unwrappedIndex = blazorJavascript.unwrap(i);

        return blazorJavascript.wrapForInterop(unwrapped[unwrappedIndex]);
    };

    blazorJavascript.constructorFunction = function(i) {
        if (!i) {
            return blazorJavascript.wrapForInterop(undefined);
        }

        const parameters = [];

        for (let i = 1; i < arguments.length; i++) {
            parameters.push(blazorJavascript.isInteropWrapped(arguments[i]) ? arguments[i].reference : arguments[i]);
        }

        if (typeof i === 'string') {
            const type = window.eval(i);
            return blazorJavascript.wrapForInterop(new type(...parameters));
        }

        if (blazorJavascript.isInteropWrapped(i)) {
            const type = i.reference;
            return blazorJavascript.wrapForInterop(new type(...parameters));
        }

        return blazorJavascript.wrapForInterop(new i(...parameters));
    };

    blazorJavascript.isInteropWrapped = function(input) {
        if (!input) {
            return false;
        }

        if (typeof input !== "object") {
            return false;
        }

        if (typeof input['marker'] !== "symbol") {
            return false;
        }

        return input['marker'] === blazorJavascript.interopMarkerSymbol;
    };

    blazorJavascript.invokeFunction = function (s, t) {
        let self = blazorJavascript.unwrap(s);

        if (!self) {
            return undefined;
        }

        const parameters = [];

        for (let i = 2; i < arguments.length; i++) {
            parameters.push(blazorJavascript.unwrap(arguments[i]));
        }

        let thisReference = blazorJavascript.unwrap(t);

        if (!thisReference) {
            // TODO: Should we pass "window" here instead?
            return blazorJavascript.wrapForInterop(self.apply(this, parameters));
        }

        return blazorJavascript.wrapForInterop(self.apply(thisReference, parameters));
    };

    for (const key in blazorJavascript) {
        if (blazorJavascript.hasOwnProperty(key)) {
            window['__blazorJavascript_' + key] = blazorJavascript[key];
        }
    }

    window.BlazorJavascript = blazorJavascript;
    console.log("BlazorJavascript initialized!");
}
