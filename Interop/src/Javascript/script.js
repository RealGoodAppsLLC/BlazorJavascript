if (typeof window['blazorJavascript'] === 'undefined') {
    let blazorJavascript = {};

    blazorJavascript.interopMarkerSymbol = Symbol();

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

    blazorJavascript.constructArray = function() {
        return blazorJavascript.wrapForInterop([]);
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

    const parametersWithIdentifiers = {};
    const returnValuesWithIdentifiers = {};

    blazorJavascript.generateUuid = () => {
        return ([1e7]+-1e3+-4e3+-8e3+-1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    };
    
    blazorJavascript.getParamByIdentifier = function(i) {
        if (typeof parametersWithIdentifiers[i] == "undefined") {
            delete parametersWithIdentifiers[i];
            return blazorJavascript.wrapForInterop(undefined);
        }
        
        let wrapped = blazorJavascript.wrapForInterop(parametersWithIdentifiers[i]);
        delete parametersWithIdentifiers[i];
        return wrapped;
    };

    blazorJavascript.storeReturnValue = function(o) {
        let unwrapped = blazorJavascript.unwrap(o);
        let identifier = blazorJavascript.generateUuid();
        
        returnValuesWithIdentifiers[identifier] = unwrapped;
        return identifier;
    };
    
    blazorJavascript.constructAction = function(o, n) {
        function trampoline() {
            let parameters = ['InvokeAction'];
            
            for (let argIndex = 0; argIndex < n; argIndex++) {
                let parameterIdentifier = blazorJavascript.generateUuid();
                parametersWithIdentifiers[parameterIdentifier] = blazorJavascript.unwrap(arguments[argIndex]);
                
                parameters.push(parameterIdentifier);
            }

            o.invokeMethod.apply(o, parameters);
        }
        
        return blazorJavascript.wrapForInterop(trampoline);
    };

    blazorJavascript.constructFunc = function(o, n) {
        function trampoline() {
            let parameters = ['InvokeFunc'];

            for (let argIndex = 0; argIndex < n; argIndex++) {
                let parameterIdentifier = blazorJavascript.generateUuid();
                parametersWithIdentifiers[parameterIdentifier] = arguments[argIndex];

                parameters.push(parameterIdentifier);
            }

            let returnValueIdentifier = o.invokeMethod.apply(o, parameters);
            let returnValue = returnValuesWithIdentifiers[returnValueIdentifier];
            delete returnValuesWithIdentifiers[returnValueIdentifier];
            return returnValue;
        }

        return blazorJavascript.wrapForInterop(trampoline);
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

    blazorJavascript.indexerGetFunction = function (o, k) {
        if (blazorJavascript.checkIsNull(o) || blazorJavascript.checkIsUndefined(o)) {
            return blazorJavascript.wrapForInterop(undefined);
        }

        let r = blazorJavascript.isInteropWrapped(o) ? o.reference : o;
        let z = blazorJavascript.isInteropWrapped(k) ? k.reference : k;

        if (typeof r[z] === "undefined") {
            return blazorJavascript.wrapForInterop(undefined);
        }

        return blazorJavascript.wrapForInterop(r[z]);
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

    blazorJavascript.indexerSetFunction = function (o, k, v) {
        if (blazorJavascript.checkIsNull(o) || blazorJavascript.checkIsUndefined(o)) {
            return;
        }

        let r = blazorJavascript.isInteropWrapped(o) ? o.reference : o;
        let z = blazorJavascript.isInteropWrapped(k) ? k.reference : k;

        r[z] = blazorJavascript.unwrap(v);
    };

    blazorJavascript.arrayItemAtIndex = function(o, i) {
        let unwrapped = blazorJavascript.unwrap(o);
        let unwrappedIndex = blazorJavascript.unwrap(i);

        return blazorJavascript.wrapForInterop(unwrapped[unwrappedIndex]);
    };

    blazorJavascript.arrayPush = function(a, i) {
        let unwrapped = blazorJavascript.unwrap(a);
        let unwrappedItem = blazorJavascript.unwrap(i);

        return blazorJavascript.wrapForInterop(unwrapped.push(unwrappedItem));
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
