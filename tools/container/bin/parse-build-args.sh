ABORT=0
DEBUG=0
OUTPUT=""

ARCH=x64
BRANCH=main
TEST=0
SAXONJ=0
SAXONC=0
SAXONCS=0
PYTHON=0
WHEELS=0
HE=0
PE=0
EE=0
CLEAN=0
DOXYGEN=0
PYDOC=0

parse_truthy() {
    case $1 in
        yes | true | 1)
            echo "1"
            return 0
            ;;
        no | false | 0)
            echo "0"
            return 0
            ;;
        *)
            echo "Error, '$1' in '$2' must be 'yes' or 'no'" 1>&2
            return 1
    esac
}

parse_build_args() {
    local value=""
    local opt=""

    for opt in "$@"; do
        value=`echo $opt | sed "s/^.*://"`
        case $opt in
            -d | -debug)
                DEBUG=1
                ;;
            -arch:*)
                ARCH=$value
                ;;
            -branch:*)
                BRANCH=$value
                ;;
            -test:*)
                TEST=$(parse_truthy $value $opt)
                ;;
            -saxonj:*)
                SAXONJ=$(parse_truthy $value $opt)
                ;;
            -saxoncs:*)
                SAXONCS=$(parse_truthy $value $opt)
                ;;
            -saxonc:*)
                SAXONC=$(parse_truthy $value $opt)
                ;;
            -python:*)
                PYTHON=$(parse_truthy $value $opt)
                ;;
            -wheels:*)
                WHEELS=$(parse_truthy $value $opt)
                ;;
            -he:*)
                HE=$(parse_truthy $value $opt)
                ;;
            -pe:*)
                PE=$(parse_truthy $value $opt)
                ;;
            -ee:*)
                EE=$(parse_truthy $value $opt)
                ;;
            -output:*)
                OUTPUT=$value
                ;;
            -clean:*)
                CLEAN=$(parse_truthy $value $opt)
                ;;
            -doxygen:*)
                DOXYGEN=$(parse_truthy $value $opt)
                ;;
            -pydoc:*)
                PYDOC=$(parse_truthy $value $opt)
                ;;
            *)
                echo "Unrecognized option: $opt"
                ABORT=1
                ;;
        esac
        if [ $? != 0 ]; then
            ABORT=1
        fi
    done

    if [ "$SAXONJ" = "0" -a "$SAXONCS" = "0" -a "$SAXONC" = "0" -a "$PYTHON" = "0" -a "$WHEELS" = "0" ]; then
        SAXONJ=1
    fi

    if [ "$HE" = "0" -a "$PE" = "0" -a "$EE" = "0" ]; then
        HE=1
        PE=1
        EE=1
    fi
}
